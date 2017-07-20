create table tb_Role(
rid int primary key identity(1,1) not null,
rname nvarchar(20),
risDelete int default(0) not null
)

create table tb_User(
id int primary key identity(1,1) not null,
name nvarchar(20),
mobile nvarchar(20),
password varchar(20),
sex int default(0) not null
)

alter table tb_User add RoleID int default(0) not null


alter table tb_User add isDelete int default(0) not null