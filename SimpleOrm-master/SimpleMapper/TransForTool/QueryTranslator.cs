using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.UI.WebControls;

namespace SimpleMapper.TransForTool
{
    public class QueryTranslator : ExpressionVisitor
    {
        StringBuilder sb;

        public string Translate(Expression expression)
        {
            this.sb = new StringBuilder();
            this.Visit(expression);
            return this.sb.ToString();
        }

        public string TranslateWhere(Expression expression)
        {
            this.sb = new StringBuilder();
            this.Visit(expression);
            return "where "+ this.sb.ToString();
        }
        public string TranslateUpdate(Expression expression)
        {
            this.sb = new StringBuilder();
            this.Visit(expression);
            return this.sb.ToString();
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(QueryExtensions) && m.Method.Name == "Where")
            {
                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                this.Visit(lambda.Body);
                return m;
            }
            throw new NotSupportedException(string.Format("方法{0}不支持", m.Method.Name));
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    sb.Append(" NOT ");
                    this.Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("运算{0}不支持", u.NodeType));
            }
            return u;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            sb.Append("(");
            this.Visit(b.Left);
            switch (b.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.And:
                    sb.Append(" AND ");
                    break;
                case ExpressionType.OrElse:
                case ExpressionType.Or:
                    sb.Append(" OR");
                    break;
                case ExpressionType.Equal:
                    sb.Append(" = ");
                    break;
                case ExpressionType.NotEqual:
                    sb.Append(" <> ");
                    break;
                case ExpressionType.LessThan:
                    sb.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sb.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    sb.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" >= ");
                    break;
                default:
                    throw new NotSupportedException(string.Format("运算符{0}不支持", b.NodeType));
            }
            this.Visit(b.Right);
            sb.Append(")");
            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;
            if (q != null)
            {
                // 我们假设我们那个Queryable就是对应的表
                sb.Append("SELECT * FROM ");
                sb.Append(q.ElementType.Name);
            }
            else if (c.Value == null)
            {
                sb.Append("NULL");
            }
            else
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        sb.Append(((bool)c.Value) ? " 1=1" : " 1=0");
                        break;
                    case TypeCode.String:
                        sb.Append("'");
                        sb.Append(c.Value);
                        sb.Append("'");
                        break;
                    case TypeCode.Object:
                        throw new NotSupportedException(string.Format("常量{0}不支持", c.Value));
                    default:
                        sb.Append(c.Value);
                        break;
                }
            }
            return c;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Constant)
            {
                int num = 0;
                string str = Expression.Lambda(m).Compile().DynamicInvoke().ToString();
                if (!int.TryParse(str, out num))
                {
                    sb.Append("'" + str + "'");
                }
                else
                {
                    sb.Append(str);
                }
                return m;
            }
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.MemberAccess)
            {
                int num = 0;
                string str = Expression.Lambda(m).Compile().DynamicInvoke().ToString();
                if (!int.TryParse(str, out num))
                {
                    sb.Append("'" + str + "'");
                }
                else
                {
                    sb.Append(str);
                }
                return m;
            }
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                sb.Append(m.Member.Name);
                return m;
            }
            throw new NotSupportedException(string.Format("成员{0}不支持", m.Member.Name));
        }
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            sb.Append(node.Member.Name);
            sb.Append("=");
            var ex = this.Visit(node.Expression);
            sb.Append(",");
            return node.Update(ex);
        }
        protected override Expression VisitNew(NewExpression node)
        {
            sb.Append("Update " + node.ToString().Replace("new ", "").Replace("()","")+ " set ");
            return node.Update(this.Visit(node.Arguments));
        }
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var newExpression = this.VisitAndConvert<NewExpression>(node.NewExpression, "VisitMemberInit"); var memberExpression = Visit<MemberBinding>(node.Bindings, new Func<MemberBinding, MemberBinding>(this.VisitMemberBinding));
            sb.Remove(sb.Length-1,1);
            return node.Update(newExpression, memberExpression);
        }
    }
}
