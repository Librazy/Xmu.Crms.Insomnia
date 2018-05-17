# 项目结构

## Xmu.Crms.Insomnia.sln

解决方案文件，负责管理项目之间的依赖关系并指定项目入口点

## Xmu.Crms.Insomnia

入口项目，配置依赖注入与 Web 服务器，调用需要的 Service 的注册方法。

## Xmu.Crms.Shared

依赖库，存放接口、实体与共用的中间件。  
**所有小组的 Xmu.Crms.Shared 项目均由本小组提供**，包含了技术难度较高的依赖注入控制、鉴权中间件、定时器、数据库连接池等功能。

## Xmu.Crms.Web.Insomnia

PC 端 Web 界面。

## Xmu.Crms.Mobile.HighGrade

手机端 Web 界面。

## Xmu.Crms.Services.*

来自三个小组的服务层。

## Xmu.Crms.Insomnia.XUnitTest

单元测试项目。

## Xmu.Crms.API.Insomnia

RESTFul API 接口。