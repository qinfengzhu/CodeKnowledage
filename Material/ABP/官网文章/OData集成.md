[TOC]

## 介绍

在中odata.org网站，OData被定义为“一种开放协议，允许以简单和标准的方式创建和使用可查询和可互操作的restfulapi”. 可以将OData与ASP.NET Boilerplate。Abp.Web.Api.OData-nuget包简化了它的使用。

## 启动

### 安装Nuget包

```
Install-Package Abp.Web.Api.OData
```

### 设置模块依赖

```
[DependsOn(typeof(AbpWebApiODataModule))]
public class MyProjectWebApiModule:AbpModule
{
  ......
}
```

### 配置你的实体(Configure Your Entities)

OData需要声明可以用作OData资源的实体。我们应该在模块的PreInitialize方法中进行，如下所示：

```
[DependsOn(typeof(AbpWebApiODataModule))]
public class MyProjectWebApiModule : AbpModule
{
    public override void PreInitialize()
    {
        var builder = Configuration.Modules.AbpWebApiOData().ODataModelBuilder;

        //Configure your entities here...
        builder.EntitySet<Person>("Persons");
    }

    ...
}
```

在这里，我们得到ODataModelBuilder引用并设置Person实体。可以使用EntitySet添加其他类似的实体。有关生成器的详细信息，请参见[OData文档](https://docs.microsoft.com/en-us/aspnet/web-api/overview/odata-support-in-aspnet-web-api/odata-v4/create-an-odata-v4-endpoint)。

## 创建控制器(Create Controllers)

Abp.Web.Api.OData nuget包包括AbpODataEntityController基类（它扩展了标准的ODataController），可以更轻松地创建控制器。为Person实体创建OData端点的示例：

```
public class PersonsController : AbpODataEntityController<Person>
{
    public PersonsController(IRepository<Person> repository)
        : base(repository)
    {
    }
}
```
就那么简单。AbpODataEntityController的所有方法都是虚拟的。这意味着您可以覆盖Get、Post、Put、Patch、Delete和其他操作，并添加自己的逻辑。

## 配置(Configuration)

Abp.Web.Api.OData自动调用HttpConfiguration.MapODataServiceRoute方法采用常规配置。如果需要，可以设置Configuration.Modules.AbpWebApiOData（）.MapAction可以自己绘制OData路线。

## 实例(Examples)

这里有一些对上面定义的控制器的请求示例。假设应用程序在http://localhost：61842。我们将展示一些基本知识。由于OData是一个标准协议，因此可以在web上轻松地找到更高级的示例。

### Getting List of entities

Getting all people

Rqeuest
```
Get  http://localhost:61842/odata/Persons
```

Response
```
{
  "@odata.context":"http://localhost:61842/odata/$metadata#Persons","value":[
    {
      "Name":"Douglas Adams","IsDeleted":false,"DeleterUserId":null,"DeletionTime":null,"LastModificationTime":null,"LastModifierUserId":null,"CreationTime":"2015-11-07T20:12:39.363+03:00","CreatorUserId":null,"Id":1
    },{
      "Name":"John Nash","IsDeleted":false,"DeleterUserId":null,"DeletionTime":null,"LastModificationTime":null,"LastModifierUserId":null,"CreationTime":"2015-11-07T20:12:39.363+03:00","CreatorUserId":null,"Id":2
    }
  ]
}
```

### Getting a Single Entity

获取Id=2的人员信息

Request
```
GET http://localhost:61842/odata/Persons(2)
```
Response
```
{
  "@odata.context":"http://localhost:61842/odata/$metadata#Persons/$entity","Name":"John Nash","IsDeleted":false,"DeleterUserId":null,"DeletionTime":null,"LastModificationTime":null,"LastModifierUserId":null,"CreationTime":"2015-11-07T20:12:39.363+03:00","CreatorUserId":null,"Id":2
}
```

###  根据导航属性获取一个实体信息(Getting a Single Entity With Navigation Properties)

获取Id=1的人员信息，包括他的电话号码

Request
```
GET http://localhost:61842/odata/Persons(1)?$expand=Phones
```
Response
```
{
  "@odata.context":"http://localhost:61842/odata/$metadata#Persons/$entity","Name":"Douglas Adams","IsDeleted":false,"DeleterUserId":null,"DeletionTime":null,"LastModificationTime":null,"LastModifierUserId":null,"CreationTime":"2015-11-07T20:12:39.363+03:00","CreatorUserId":null,"Id":1,"Phones":[
    {
      "PersonId":1,"Type":"Mobile","Number":"4242424242","CreationTime":"2015-11-07T20:12:39.363+03:00","CreatorUserId":null,"Id":1
    },{
      "PersonId":1,"Type":"Mobile","Number":"2424242424","CreationTime":"2015-11-07T20:12:39.363+03:00","CreatorUserId":null,"Id":2
    }
  ]
}
```
### 查询

Request
```
GET http://localhost:61842/odata/Persons?$filter=Name eq 'Douglas Adams'&$orderby=CreationTime&$top=2
```
Response
```
{
  "@odata.context":"http://localhost:61842/odata/$metadata#Persons","value":[
    {
      "Name":"Douglas Adams","IsDeleted":false,"DeleterUserId":null,"DeletionTime":null,"LastModificationTime":null,"LastModifierUserId":null,"CreationTime":"2015-11-07T20:12:39.363+03:00","CreatorUserId":null,"Id":1
    },{
      "Name":"Douglas Adams","IsDeleted":false,"DeleterUserId":null,"DeletionTime":null,"LastModificationTime":null,"LastModifierUserId":null,"CreationTime":"2016-01-12T20:29:03+02:00","CreatorUserId":null,"Id":3
    }
  ]
}
```
OData支持分页、排序、过滤、投影等等。更多信息请参见它自己的文档。

### 创建一个新实体

Request  ,"Content-Type" 头为 "application/json"
```
POST http://localhost:61842/odata/Persons

{
    Name: "Galileo Galilei"
}
```
Response
```
{
  "@odata.context": "http://localhost:61842/odata/$metadata#Persons/$entity",
  "Name": "Galileo Galilei",
  "IsDeleted": false,
  "DeleterUserId": null,
  "DeletionTime": null,
  "LastModificationTime": null,
  "LastModifierUserId": null,
  "CreationTime": "2016-01-12T20:36:04.1628263+02:00",
  "CreatorUserId": null,
  "Id": 4
}
```

### 获取MetaData

Request
```
GET http://localhost:61842/odata/$metadata
```
Response
```
<?xml version="1.0" encoding="utf-8"?>

<edmx:Edmx Version="4.0" xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx">

    <edmx:DataServices>

        <Schema Namespace="AbpODataDemo.People" xmlns="http://docs.oasis-open.org/odata/ns/edm">

            <EntityType Name="Person">

                <Key>

                    <PropertyRef Name="Id" />

                </Key>

                <Property Name="Name" Type="Edm.String" Nullable="false" />

                <Property Name="IsDeleted" Type="Edm.Boolean" Nullable="false" />

                <Property Name="DeleterUserId" Type="Edm.Int64" />

                <Property Name="DeletionTime" Type="Edm.DateTimeOffset" />

                <Property Name="LastModificationTime" Type="Edm.DateTimeOffset" />

                <Property Name="LastModifierUserId" Type="Edm.Int64" />

                <Property Name="CreationTime" Type="Edm.DateTimeOffset" Nullable="false" />

                <Property Name="CreatorUserId" Type="Edm.Int64" />

                <Property Name="Id" Type="Edm.Int32" Nullable="false" />

                <NavigationProperty Name="Phones" Type="Collection(AbpODataDemo.People.Phone)" />

            </EntityType>

            <EntityType Name="Phone">

                <Key>

                    <PropertyRef Name="Id" />

                </Key>

                <Property Name="PersonId" Type="Edm.Int32" />

                <Property Name="Type" Type="AbpODataDemo.People.PhoneType" Nullable="false" />

                <Property Name="Number" Type="Edm.String" Nullable="false" />

                <Property Name="CreationTime" Type="Edm.DateTimeOffset" Nullable="false" />

                <Property Name="CreatorUserId" Type="Edm.Int64" />

                <Property Name="Id" Type="Edm.Int32" Nullable="false" />

                <NavigationProperty Name="Person" Type="AbpODataDemo.People.Person">

                    <ReferentialConstraint Property="PersonId" ReferencedProperty="Id" />

                </NavigationProperty>

            </EntityType>

            <EnumType Name="PhoneType">

                <Member Name="Unknown" Value="0" />

                <Member Name="Mobile" Value="1" />

                <Member Name="Home" Value="2" />

                <Member Name="Office" Value="3" />

            </EnumType>

        </Schema>

        <Schema Namespace="Default" xmlns="http://docs.oasis-open.org/odata/ns/edm">

            <EntityContainer Name="Container">

                <EntitySet Name="Persons" EntityType="AbpODataDemo.People.Person" />

            </EntityContainer>

        </Schema>

    </edmx:DataServices>

</edmx:Edmx>
```
