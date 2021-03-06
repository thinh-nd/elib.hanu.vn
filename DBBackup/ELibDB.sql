USE [master]
GO
/****** Object:  Database [HanuElibrary2012]    Script Date: 06/18/2012 14:33:43 ******/
CREATE DATABASE [HanuElibrary2012] ON  PRIMARY 
( NAME = N'iwndSuite', FILENAME = N'c:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\HanuElibrary2012.mdf' , SIZE = 4096KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'iwndSuite_log', FILENAME = N'c:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\HanuElibrary2012.LDF' , SIZE = 768KB , MAXSIZE = UNLIMITED, FILEGROWTH = 10%)
GO
ALTER DATABASE [HanuElibrary2012] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [HanuElibrary2012].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [HanuElibrary2012] SET ANSI_NULL_DEFAULT OFF
GO
ALTER DATABASE [HanuElibrary2012] SET ANSI_NULLS OFF
GO
ALTER DATABASE [HanuElibrary2012] SET ANSI_PADDING OFF
GO
ALTER DATABASE [HanuElibrary2012] SET ANSI_WARNINGS OFF
GO
ALTER DATABASE [HanuElibrary2012] SET ARITHABORT OFF
GO
ALTER DATABASE [HanuElibrary2012] SET AUTO_CLOSE ON
GO
ALTER DATABASE [HanuElibrary2012] SET AUTO_CREATE_STATISTICS ON
GO
ALTER DATABASE [HanuElibrary2012] SET AUTO_SHRINK OFF
GO
ALTER DATABASE [HanuElibrary2012] SET AUTO_UPDATE_STATISTICS ON
GO
ALTER DATABASE [HanuElibrary2012] SET CURSOR_CLOSE_ON_COMMIT OFF
GO
ALTER DATABASE [HanuElibrary2012] SET CURSOR_DEFAULT  GLOBAL
GO
ALTER DATABASE [HanuElibrary2012] SET CONCAT_NULL_YIELDS_NULL OFF
GO
ALTER DATABASE [HanuElibrary2012] SET NUMERIC_ROUNDABORT OFF
GO
ALTER DATABASE [HanuElibrary2012] SET QUOTED_IDENTIFIER OFF
GO
ALTER DATABASE [HanuElibrary2012] SET RECURSIVE_TRIGGERS OFF
GO
ALTER DATABASE [HanuElibrary2012] SET  DISABLE_BROKER
GO
ALTER DATABASE [HanuElibrary2012] SET AUTO_UPDATE_STATISTICS_ASYNC OFF
GO
ALTER DATABASE [HanuElibrary2012] SET DATE_CORRELATION_OPTIMIZATION OFF
GO
ALTER DATABASE [HanuElibrary2012] SET TRUSTWORTHY OFF
GO
ALTER DATABASE [HanuElibrary2012] SET ALLOW_SNAPSHOT_ISOLATION OFF
GO
ALTER DATABASE [HanuElibrary2012] SET PARAMETERIZATION SIMPLE
GO
ALTER DATABASE [HanuElibrary2012] SET READ_COMMITTED_SNAPSHOT OFF
GO
ALTER DATABASE [HanuElibrary2012] SET HONOR_BROKER_PRIORITY OFF
GO
ALTER DATABASE [HanuElibrary2012] SET  READ_WRITE
GO
ALTER DATABASE [HanuElibrary2012] SET RECOVERY SIMPLE
GO
ALTER DATABASE [HanuElibrary2012] SET  MULTI_USER
GO
ALTER DATABASE [HanuElibrary2012] SET PAGE_VERIFY CHECKSUM
GO
ALTER DATABASE [HanuElibrary2012] SET DB_CHAINING OFF
GO
USE [HanuElibrary2012]
GO
/****** Object:  User [qml]    Script Date: 06/18/2012 14:33:43 ******/
CREATE USER [qml] WITHOUT LOGIN WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  FullTextCatalog [TranslationCatalog]    Script Date: 06/18/2012 14:33:44 ******/
CREATE FULLTEXT CATALOG [TranslationCatalog]AUTHORIZATION [dbo]
GO
/****** Object:  Table [dbo].[core_Configs]    Script Date: 06/18/2012 14:33:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[core_Configs](
	[ConfigId] [bigint] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](50) NOT NULL,
	[Value] [nvarchar](250) NOT NULL,
 CONSTRAINT [PK_Configs] PRIMARY KEY CLUSTERED 
(
	[ConfigId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[core_Areas]    Script Date: 06/18/2012 14:33:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[core_Areas](
	[AreaId] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_core_Areas] PRIMARY KEY CLUSTERED 
(
	[AreaId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[app_Features]    Script Date: 06/18/2012 14:33:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[app_Features](
	[FeatureId] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NULL,
	[Description] [nvarchar](100) NULL,
 CONSTRAINT [PK_app_Features] PRIMARY KEY CLUSTERED 
(
	[FeatureId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[app_Applications]    Script Date: 06/18/2012 14:33:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[app_Applications](
	[ApplicationId] [bigint] IDENTITY(1,1) NOT NULL,
	[DomainName] [nvarchar](250) NOT NULL,
	[CurrentTheme] [nvarchar](100) NULL,
 CONSTRAINT [PK_core_Applications] PRIMARY KEY CLUSTERED 
(
	[ApplicationId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DocumentFormat]    Script Date: 06/18/2012 14:33:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DocumentFormat](
	[DocumentFormatID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](250) NULL,
	[Description] [ntext] NULL,
	[MIME] [nvarchar](250) NULL,
	[FileFormat] [varchar](250) NULL,
	[Avatar] [nvarchar](250) NULL,
	[Status] [bit] NULL,
 CONSTRAINT [PK_MediaType] PRIMARY KEY CLUSTERED 
(
	[DocumentFormatID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DocumentCategories]    Script Date: 06/18/2012 14:33:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DocumentCategories](
	[CategoryID] [int] IDENTITY(1,1) NOT NULL,
	[ParentID] [bigint] NOT NULL,
	[CategoryName] [nvarchar](250) NULL,
	[Position] [bigint] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedUser] [bigint] NULL,
	[LastModifiedDate] [datetime] NULL,
	[LastModifiedUser] [bigint] NULL,
 CONSTRAINT [PK_BookCategories] PRIMARY KEY CLUSTERED 
(
	[CategoryID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[core_Frontend_Menu]    Script Date: 06/18/2012 14:33:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[core_Frontend_Menu](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](250) NULL,
	[Description] [ntext] NULL,
	[IsMain] [bit] NULL,
	[Code] [nvarchar](50) NULL,
 CONSTRAINT [PK_Frontend_Menu] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[ec_FilterAttribute]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[ec_FilterAttribute]
	-- Add the parameters for the stored procedure here
	@attributeIds varchar(250),
	@command varchar(10),
	@value bigint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	CREATE TABLE #TempList
	(
		AttributeId bigint
	)

	DECLARE @AttributeId varchar(10), @Pos int

	SET @attributeIds = LTRIM(RTRIM(@attributeIds))+ ','
	SET @Pos = CHARINDEX(',', @attributeIds, 1)

	IF REPLACE(@attributeIds, ',', '') <> ''
	BEGIN
		WHILE @Pos > 0
		BEGIN
			SET @AttributeId = LTRIM(RTRIM(LEFT(@attributeIds, @Pos - 1)))
			IF @AttributeId <> ''
			BEGIN
				INSERT INTO #TempList (AttributeId) VALUES (CAST(@AttributeId AS bigint)) --Use Appropriate conversion
			END
			SET @attributeIds = RIGHT(@attributeIds, LEN(@attributeIds) - @Pos)
			SET @Pos = CHARINDEX(',', @attributeIds, 1)

		END
	END	
    -- Insert statements for procedure here
    if @command = 'g'
    begin
		select * from ec_AttributeValues a
			join #TempList t
			on a.AttributeId = t.AttributeId
		where ISNUMERIC(a.Value) = 1 and CONVERT(bigint, CONVERT(float, a.Value)) >= @value;
	end
	else if @command = 'l'
	begin
		select * from ec_AttributeValues a
			join #TempList t
			on a.AttributeId = t.AttributeId
		where ISNUMERIC(a.Value) = 1 and CONVERT(bigint, CONVERT(float, a.Value)) <= @value;
	end
END
GO
/****** Object:  Table [dbo].[ViewHistory]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ViewHistory](
	[ViewHistoryId] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[BookId] [bigint] NOT NULL,
	[ViewDate] [datetime] NOT NULL,
	[DocumentTitle] [nvarchar](100) NULL,
	[DocumentPublisher] [nvarchar](100) NULL,
 CONSTRAINT [PK_ViewHistory] PRIMARY KEY CLUSTERED 
(
	[ViewHistoryId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SearchHistory]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SearchHistory](
	[SearchHistoryId] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[Keyword] [nvarchar](100) NOT NULL,
	[SearchDate] [datetime] NOT NULL,
 CONSTRAINT [PK_SearchHistory] PRIMARY KEY CLUSTERED 
(
	[SearchHistoryId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[core_Locales]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[core_Locales](
	[LocaleId] [bigint] IDENTITY(1,1) NOT NULL,
	[ApplicationId] [bigint] NULL,
	[Name] [nvarchar](20) NULL,
	[Description] [nvarchar](50) NULL,
	[Icon] [nvarchar](500) NULL,
 CONSTRAINT [PK_Locales] PRIMARY KEY CLUSTERED 
(
	[LocaleId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DocumentStatistic]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DocumentStatistic](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Value] [float] NULL,
	[CateID] [int] NULL,
	[Type] [nvarchar](100) NULL,
	[Year] [bigint] NULL,
 CONSTRAINT [PK_DocumentStatistic] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DocumentsFile]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DocumentsFile](
	[DocumentID] [bigint] IDENTITY(1,1) NOT NULL,
	[FileName] [nvarchar](500) NULL,
	[CategoryID] [int] NULL,
	[FormatID] [int] NULL,
	[FileSource] [nvarchar](500) NULL,
	[Thumbnail] [nvarchar](500) NULL,
	[BookFee] [float] NULL,
	[Size] [float] NULL,
	[ViewCount] [bigint] NULL,
	[Status] [nvarchar](100) NULL,
	[IsHasInfo] [bit] NULL,
	[IsDeleted] [bit] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedUser] [nvarchar](250) NULL,
	[LastModifiedDate] [datetime] NULL,
	[LastModifiedUser] [nvarchar](250) NULL,
 CONSTRAINT [PK_BookMappingInfo] PRIMARY KEY CLUSTERED 
(
	[DocumentID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[core_Controllers]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[core_Controllers](
	[ControllerId] [bigint] IDENTITY(1,1) NOT NULL,
	[AreaId] [bigint] NULL,
	[Name] [nvarchar](50) NULL,
 CONSTRAINT [PK_core_Controllers] PRIMARY KEY CLUSTERED 
(
	[ControllerId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[app_ApplicationFeatures]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[app_ApplicationFeatures](
	[ApplicationId] [bigint] NOT NULL,
	[FeatureId] [bigint] NOT NULL,
 CONSTRAINT [PK_app_ApplicationFeatures] PRIMARY KEY CLUSTERED 
(
	[ApplicationId] ASC,
	[FeatureId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[app_Pages]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[app_Pages](
	[PageId] [bigint] IDENTITY(1,1) NOT NULL,
	[ParentId] [bigint] NULL,
	[ApplicationId] [bigint] NOT NULL,
	[CustomUrl] [nvarchar](250) NULL,
	[Title] [nvarchar](100) NULL,
	[Order] [int] NULL,
	[Alias] [nvarchar](100) NULL,
 CONSTRAINT [PK_app_Pages] PRIMARY KEY CLUSTERED 
(
	[PageId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[app_StaticItems]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[app_StaticItems](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ApplicationId] [bigint] NOT NULL,
	[Code] [nvarchar](50) NOT NULL,
	[Title] [nvarchar](250) NULL,
	[Body] [nvarchar](max) NULL,
	[Position] [int] NULL,
 CONSTRAINT [PK_Article_Static_Items] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[auth_Users]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[auth_Users](
	[UserId] [bigint] IDENTITY(1,1) NOT NULL,
	[ApplicationId] [bigint] NULL,
	[Username] [nvarchar](100) NOT NULL,
	[Password] [nvarchar](100) NOT NULL,
	[CreatedDate] [datetime] NULL,
	[LastLoginDate] [datetime] NULL,
	[AuthToken] [nvarchar](100) NULL,
 CONSTRAINT [PK_auth_Users] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[auth_Roles]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[auth_Roles](
	[RoleId] [bigint] IDENTITY(1,1) NOT NULL,
	[ApplicationId] [bigint] NULL,
	[RoleName] [nvarchar](100) NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_auth_Roles] PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[core_Actions]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[core_Actions](
	[ActionId] [bigint] IDENTITY(1,1) NOT NULL,
	[ControllerId] [bigint] NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Description] [nvarchar](100) NULL,
	[Template] [nvarchar](100) NULL,
 CONSTRAINT [PK_core_Actions] PRIMARY KEY CLUSTERED 
(
	[ActionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[auth_UsersInRoles]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[auth_UsersInRoles](
	[RoleId] [bigint] NOT NULL,
	[UserId] [bigint] NOT NULL,
 CONSTRAINT [PK_auth_UsersInRoles] PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC,
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[auth_RolePermissions]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[auth_RolePermissions](
	[RolePermissionId] [bigint] IDENTITY(1,1) NOT NULL,
	[RoleId] [bigint] NOT NULL,
	[Action] [nvarchar](50) NOT NULL,
	[Controller] [nvarchar](50) NOT NULL,
	[Area] [nvarchar](50) NULL,
 CONSTRAINT [PK_RolePermissions] PRIMARY KEY CLUSTERED 
(
	[RolePermissionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[app_FeatureControllers]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[app_FeatureControllers](
	[FeatureId] [bigint] NOT NULL,
	[ControllerId] [bigint] NOT NULL,
 CONSTRAINT [PK_app_FeatureControllers_1] PRIMARY KEY CLUSTERED 
(
	[FeatureId] ASC,
	[ControllerId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Profile]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Profile](
	[UserId] [bigint] NOT NULL,
	[Balance] [float] NOT NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedUser] [bigint] NULL,
	[LastModifiedDate] [datetime] NULL,
	[LastModifiedUser] [bigint] NULL,
	[Status] [bit] NOT NULL,
	[Email] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Profile] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[core_Translations]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[core_Translations](
	[TranslationId] [bigint] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](250) NOT NULL,
	[LocaleId] [bigint] NOT NULL,
	[Value] [nvarchar](max) NULL,
	[Type] [nvarchar](100) NULL,
 CONSTRAINT [PK_Translations] PRIMARY KEY CLUSTERED 
(
	[TranslationId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Documents]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Documents](
	[DocumentID] [bigint] NOT NULL,
	[Title] [nvarchar](500) NULL,
	[Creator] [nvarchar](500) NULL,
	[Subject] [nvarchar](500) NULL,
	[Description] [ntext] NULL,
	[Publisher] [nvarchar](500) NULL,
	[Contributor] [nvarchar](500) NULL,
	[Date] [date] NULL,
	[Type] [nvarchar](500) NULL,
	[Format] [nvarchar](500) NULL,
	[Identifier] [nvarchar](500) NULL,
	[Resource] [nvarchar](500) NULL,
	[Language] [nvarchar](100) NULL,
	[Relation] [nvarchar](500) NULL,
	[Coverage] [nvarchar](500) NULL,
	[Right] [nvarchar](500) NULL,
 CONSTRAINT [PK_Books_1] PRIMARY KEY CLUSTERED 
(
	[DocumentID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DocumentOrders]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DocumentOrders](
	[OrderID] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[Status] [bit] NOT NULL,
	[OrderContent] [ntext] NOT NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedUser] [bigint] NULL,
	[LastModifiedDate] [datetime] NULL,
	[LastModifiedUser] [bigint] NULL,
	[UserEmail] [nvarchar](50) NULL,
	[DocumentName] [nvarchar](50) NULL,
	[DocumentType] [nvarchar](50) NULL,
	[DocumentCategory] [nvarchar](50) NULL,
	[DocumentCreator] [nvarchar](50) NULL,
	[DocumentPublisher] [nvarchar](50) NULL,
 CONSTRAINT [PK_DocumentOrders] PRIMARY KEY CLUSTERED 
(
	[OrderID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[Search]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[Search]
	-- Add the parameters for the stored procedure here
	@text nvarchar(250)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	set @text = CHAR(34) + @text + CHAR(34)
    -- Insert statements for procedure here
		select m.* from core_Translations m where contains(Value, @text)
END
GO
/****** Object:  Table [dbo].[core_Frontend_MenuItems]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[core_Frontend_MenuItems](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[MenuID] [bigint] NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
	[ParentID] [bigint] NULL,
	[Position] [int] NOT NULL,
	[ActionId] [bigint] NULL,
	[CustomUrl] [nvarchar](250) NULL,
	[IsCutom] [bit] NULL,
	[Parameters] [nvarchar](250) NULL,
 CONSTRAINT [PK_Frontend_MenuItems] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[app_PartialViews]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[app_PartialViews](
	[PartialViewId] [bigint] IDENTITY(1,1) NOT NULL,
	[ActionId] [bigint] NOT NULL,
	[Theme] [nvarchar](100) NOT NULL,
	[Name] [nvarchar](100) NULL,
	[Content] [nvarchar](max) NULL,
 CONSTRAINT [PK_app_Partials] PRIMARY KEY CLUSTERED 
(
	[PartialViewId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[app_PageModules]    Script Date: 06/18/2012 14:34:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[app_PageModules](
	[PageModuleId] [bigint] IDENTITY(1,1) NOT NULL,
	[PageId] [bigint] NOT NULL,
	[ActionId] [bigint] NOT NULL,
	[PositionCode] [nvarchar](100) NULL,
	[Order] [int] NULL,
 CONSTRAINT [PK_app_PageModules] PRIMARY KEY CLUSTERED 
(
	[PageModuleId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Default [DF_DocumentType_Status]    Script Date: 06/18/2012 14:33:47 ******/
ALTER TABLE [dbo].[DocumentFormat] ADD  CONSTRAINT [DF_DocumentType_Status]  DEFAULT ((1)) FOR [Status]
GO
/****** Object:  Default [DF_DocumentsExtraInfo_Thumbnail]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[DocumentsFile] ADD  CONSTRAINT [DF_DocumentsExtraInfo_Thumbnail]  DEFAULT (N'nothumbnail.png') FOR [Thumbnail]
GO
/****** Object:  Default [DF_DocumentsExtraInfo_BookFee]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[DocumentsFile] ADD  CONSTRAINT [DF_DocumentsExtraInfo_BookFee]  DEFAULT ((0)) FOR [BookFee]
GO
/****** Object:  Default [DF_DocumentsExtraInfo_ViewCount]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[DocumentsFile] ADD  CONSTRAINT [DF_DocumentsExtraInfo_ViewCount]  DEFAULT ((0)) FOR [ViewCount]
GO
/****** Object:  Default [DF_DocumentsExtraInfo_Status]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[DocumentsFile] ADD  CONSTRAINT [DF_DocumentsExtraInfo_Status]  DEFAULT (N'Không hiển thị') FOR [Status]
GO
/****** Object:  Default [DF_DocumentsFile_IsHasInfo]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[DocumentsFile] ADD  CONSTRAINT [DF_DocumentsFile_IsHasInfo]  DEFAULT ((0)) FOR [IsHasInfo]
GO
/****** Object:  Default [DF_DocumentsFile_IsDeleted]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[DocumentsFile] ADD  CONSTRAINT [DF_DocumentsFile_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
/****** Object:  Default [DF_Profile_Balance]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[Profile] ADD  CONSTRAINT [DF_Profile_Balance]  DEFAULT ((0.0)) FOR [Balance]
GO
/****** Object:  Default [DF_Profile_Status]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[Profile] ADD  CONSTRAINT [DF_Profile_Status]  DEFAULT ((0)) FOR [Status]
GO
/****** Object:  Default [DF_DocumentOrders_Status]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[DocumentOrders] ADD  CONSTRAINT [DF_DocumentOrders_Status]  DEFAULT ((0)) FOR [Status]
GO
/****** Object:  Default [DF_Frontend_MenuItems_Position]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[core_Frontend_MenuItems] ADD  CONSTRAINT [DF_Frontend_MenuItems_Position]  DEFAULT ((9999)) FOR [Position]
GO
/****** Object:  ForeignKey [FK_core_Locales_core_Applications]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[core_Locales]  WITH NOCHECK ADD  CONSTRAINT [FK_core_Locales_core_Applications] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[app_Applications] ([ApplicationId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[core_Locales] NOCHECK CONSTRAINT [FK_core_Locales_core_Applications]
GO
/****** Object:  ForeignKey [FK_DocumentStatistic_DocumentCategories]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[DocumentStatistic]  WITH CHECK ADD  CONSTRAINT [FK_DocumentStatistic_DocumentCategories] FOREIGN KEY([CateID])
REFERENCES [dbo].[DocumentCategories] ([CategoryID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DocumentStatistic] CHECK CONSTRAINT [FK_DocumentStatistic_DocumentCategories]
GO
/****** Object:  ForeignKey [FK_DocumentsFile_DocumentCategories]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[DocumentsFile]  WITH CHECK ADD  CONSTRAINT [FK_DocumentsFile_DocumentCategories] FOREIGN KEY([CategoryID])
REFERENCES [dbo].[DocumentCategories] ([CategoryID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DocumentsFile] CHECK CONSTRAINT [FK_DocumentsFile_DocumentCategories]
GO
/****** Object:  ForeignKey [FK_DocumentsFile_DocumentFormat]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[DocumentsFile]  WITH CHECK ADD  CONSTRAINT [FK_DocumentsFile_DocumentFormat] FOREIGN KEY([FormatID])
REFERENCES [dbo].[DocumentFormat] ([DocumentFormatID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DocumentsFile] CHECK CONSTRAINT [FK_DocumentsFile_DocumentFormat]
GO
/****** Object:  ForeignKey [FK_core_Controllers_core_Areas]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[core_Controllers]  WITH NOCHECK ADD  CONSTRAINT [FK_core_Controllers_core_Areas] FOREIGN KEY([AreaId])
REFERENCES [dbo].[core_Areas] ([AreaId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[core_Controllers] NOCHECK CONSTRAINT [FK_core_Controllers_core_Areas]
GO
/****** Object:  ForeignKey [FK_app_ApplicationFeatures_app_Applications]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[app_ApplicationFeatures]  WITH CHECK ADD  CONSTRAINT [FK_app_ApplicationFeatures_app_Applications] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[app_Applications] ([ApplicationId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[app_ApplicationFeatures] CHECK CONSTRAINT [FK_app_ApplicationFeatures_app_Applications]
GO
/****** Object:  ForeignKey [FK_app_ApplicationFeatures_app_Features]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[app_ApplicationFeatures]  WITH CHECK ADD  CONSTRAINT [FK_app_ApplicationFeatures_app_Features] FOREIGN KEY([FeatureId])
REFERENCES [dbo].[app_Features] ([FeatureId])
GO
ALTER TABLE [dbo].[app_ApplicationFeatures] CHECK CONSTRAINT [FK_app_ApplicationFeatures_app_Features]
GO
/****** Object:  ForeignKey [FK_app_Pages_app_Applications]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[app_Pages]  WITH CHECK ADD  CONSTRAINT [FK_app_Pages_app_Applications] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[app_Applications] ([ApplicationId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[app_Pages] CHECK CONSTRAINT [FK_app_Pages_app_Applications]
GO
/****** Object:  ForeignKey [FK_app_Pages_app_Pages]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[app_Pages]  WITH CHECK ADD  CONSTRAINT [FK_app_Pages_app_Pages] FOREIGN KEY([ParentId])
REFERENCES [dbo].[app_Pages] ([PageId])
GO
ALTER TABLE [dbo].[app_Pages] CHECK CONSTRAINT [FK_app_Pages_app_Pages]
GO
/****** Object:  ForeignKey [FK_article_Static_Items_app_Applications]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[app_StaticItems]  WITH CHECK ADD  CONSTRAINT [FK_article_Static_Items_app_Applications] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[app_Applications] ([ApplicationId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[app_StaticItems] CHECK CONSTRAINT [FK_article_Static_Items_app_Applications]
GO
/****** Object:  ForeignKey [FK_auth_Users_core_Applications]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[auth_Users]  WITH NOCHECK ADD  CONSTRAINT [FK_auth_Users_core_Applications] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[app_Applications] ([ApplicationId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[auth_Users] NOCHECK CONSTRAINT [FK_auth_Users_core_Applications]
GO
/****** Object:  ForeignKey [FK_auth_Roles_core_Applications]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[auth_Roles]  WITH NOCHECK ADD  CONSTRAINT [FK_auth_Roles_core_Applications] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[app_Applications] ([ApplicationId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[auth_Roles] NOCHECK CONSTRAINT [FK_auth_Roles_core_Applications]
GO
/****** Object:  ForeignKey [FK_core_Actions_core_Controllers]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[core_Actions]  WITH NOCHECK ADD  CONSTRAINT [FK_core_Actions_core_Controllers] FOREIGN KEY([ControllerId])
REFERENCES [dbo].[core_Controllers] ([ControllerId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[core_Actions] NOCHECK CONSTRAINT [FK_core_Actions_core_Controllers]
GO
/****** Object:  ForeignKey [FK_auth_UsersInRoles_auth_Roles]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[auth_UsersInRoles]  WITH NOCHECK ADD  CONSTRAINT [FK_auth_UsersInRoles_auth_Roles] FOREIGN KEY([RoleId])
REFERENCES [dbo].[auth_Roles] ([RoleId])
GO
ALTER TABLE [dbo].[auth_UsersInRoles] NOCHECK CONSTRAINT [FK_auth_UsersInRoles_auth_Roles]
GO
/****** Object:  ForeignKey [FK_auth_UsersInRoles_auth_Users]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[auth_UsersInRoles]  WITH NOCHECK ADD  CONSTRAINT [FK_auth_UsersInRoles_auth_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[auth_Users] ([UserId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[auth_UsersInRoles] NOCHECK CONSTRAINT [FK_auth_UsersInRoles_auth_Users]
GO
/****** Object:  ForeignKey [FK_core_RolePermissions_auth_Roles]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[auth_RolePermissions]  WITH NOCHECK ADD  CONSTRAINT [FK_core_RolePermissions_auth_Roles] FOREIGN KEY([RoleId])
REFERENCES [dbo].[auth_Roles] ([RoleId])
GO
ALTER TABLE [dbo].[auth_RolePermissions] NOCHECK CONSTRAINT [FK_core_RolePermissions_auth_Roles]
GO
/****** Object:  ForeignKey [FK_app_FeatureControllers_app_Features]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[app_FeatureControllers]  WITH CHECK ADD  CONSTRAINT [FK_app_FeatureControllers_app_Features] FOREIGN KEY([FeatureId])
REFERENCES [dbo].[app_Features] ([FeatureId])
GO
ALTER TABLE [dbo].[app_FeatureControllers] CHECK CONSTRAINT [FK_app_FeatureControllers_app_Features]
GO
/****** Object:  ForeignKey [FK_app_FeatureControllers_core_Controllers]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[app_FeatureControllers]  WITH CHECK ADD  CONSTRAINT [FK_app_FeatureControllers_core_Controllers] FOREIGN KEY([ControllerId])
REFERENCES [dbo].[core_Controllers] ([ControllerId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[app_FeatureControllers] CHECK CONSTRAINT [FK_app_FeatureControllers_core_Controllers]
GO
/****** Object:  ForeignKey [FK_Profile_auth_Users]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[Profile]  WITH CHECK ADD  CONSTRAINT [FK_Profile_auth_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[auth_Users] ([UserId])
GO
ALTER TABLE [dbo].[Profile] CHECK CONSTRAINT [FK_Profile_auth_Users]
GO
/****** Object:  ForeignKey [FK_Translations_Locales]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[core_Translations]  WITH NOCHECK ADD  CONSTRAINT [FK_Translations_Locales] FOREIGN KEY([LocaleId])
REFERENCES [dbo].[core_Locales] ([LocaleId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[core_Translations] NOCHECK CONSTRAINT [FK_Translations_Locales]
GO
/****** Object:  ForeignKey [FK_Documents_DocumentsFile]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[Documents]  WITH CHECK ADD  CONSTRAINT [FK_Documents_DocumentsFile] FOREIGN KEY([DocumentID])
REFERENCES [dbo].[DocumentsFile] ([DocumentID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Documents] CHECK CONSTRAINT [FK_Documents_DocumentsFile]
GO
/****** Object:  ForeignKey [FK_DocumentOrders_auth_Users]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[DocumentOrders]  WITH CHECK ADD  CONSTRAINT [FK_DocumentOrders_auth_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[auth_Users] ([UserId])
GO
ALTER TABLE [dbo].[DocumentOrders] CHECK CONSTRAINT [FK_DocumentOrders_auth_Users]
GO
/****** Object:  ForeignKey [FK_core_Frontend_MenuItems_core_Frontend_MenuItems]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[core_Frontend_MenuItems]  WITH NOCHECK ADD  CONSTRAINT [FK_core_Frontend_MenuItems_core_Frontend_MenuItems] FOREIGN KEY([ParentID])
REFERENCES [dbo].[core_Frontend_MenuItems] ([ID])
GO
ALTER TABLE [dbo].[core_Frontend_MenuItems] NOCHECK CONSTRAINT [FK_core_Frontend_MenuItems_core_Frontend_MenuItems]
GO
/****** Object:  ForeignKey [FK_Frontend_MenuItems_core_Actions]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[core_Frontend_MenuItems]  WITH NOCHECK ADD  CONSTRAINT [FK_Frontend_MenuItems_core_Actions] FOREIGN KEY([ActionId])
REFERENCES [dbo].[core_Actions] ([ActionId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[core_Frontend_MenuItems] NOCHECK CONSTRAINT [FK_Frontend_MenuItems_core_Actions]
GO
/****** Object:  ForeignKey [FK_Frontend_MenuItems_Frontend_Menu]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[core_Frontend_MenuItems]  WITH NOCHECK ADD  CONSTRAINT [FK_Frontend_MenuItems_Frontend_Menu] FOREIGN KEY([MenuID])
REFERENCES [dbo].[core_Frontend_Menu] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[core_Frontend_MenuItems] NOCHECK CONSTRAINT [FK_Frontend_MenuItems_Frontend_Menu]
GO
/****** Object:  ForeignKey [FK_app_PartialViews_core_Actions]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[app_PartialViews]  WITH CHECK ADD  CONSTRAINT [FK_app_PartialViews_core_Actions] FOREIGN KEY([ActionId])
REFERENCES [dbo].[core_Actions] ([ActionId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[app_PartialViews] CHECK CONSTRAINT [FK_app_PartialViews_core_Actions]
GO
/****** Object:  ForeignKey [FK_app_PageModules_app_Pages]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[app_PageModules]  WITH CHECK ADD  CONSTRAINT [FK_app_PageModules_app_Pages] FOREIGN KEY([PageId])
REFERENCES [dbo].[app_Pages] ([PageId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[app_PageModules] CHECK CONSTRAINT [FK_app_PageModules_app_Pages]
GO
/****** Object:  ForeignKey [FK_app_PageModules_core_Actions]    Script Date: 06/18/2012 14:34:00 ******/
ALTER TABLE [dbo].[app_PageModules]  WITH CHECK ADD  CONSTRAINT [FK_app_PageModules_core_Actions] FOREIGN KEY([ActionId])
REFERENCES [dbo].[core_Actions] ([ActionId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[app_PageModules] CHECK CONSTRAINT [FK_app_PageModules_core_Actions]
GO
