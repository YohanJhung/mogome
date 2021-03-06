USE [C14]
GO
/****** Object:  StoredProcedure [dbo].[AppUsers_Admin_SelectAllPaged]    Script Date: 4/26/2016 2:17:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER proc [dbo].[AppUsers_Admin_SelectAllPaged]
		@PageSize int
		,@PageNum int
		,@AccountId int
		,@rolename nvarchar(100)
		,@searchTerm  nvarchar(100) = null

/*
declare
		@PageSize int = 4
		,@PageNum int = 1
		,@AccountId int = 2
		,@rolename nvarchar(100) = 'Admin'

execute dbo.AppUsers_Admin_SelectAllPaged
		@PageSize
		,@PageNum
		,@AccountId
		,@rolename
*/

as


	
	if(@rolename = 'NotSet')
	BEGIN
		set @rolename = null
	END

begin

	
	;with accountUsers as(
		select	au.LastName
				,au.FirstName
				,au.UserId
				,anu.Email
				,au.AccountId
				,a.Name as AccountName
				,aur.RoleId
				,ar.Name as RoleName
			
		from dbo.AppUsers as au 
				inner join dbo.AspNetUsers as anu
					on au.UserId = anu.Id

				
				inner join dbo.Accounts as a
					on au.AccountId = a.Id

				left join dbo.AspNetUserRoles as aur
					on au.UserId = aur.UserId
				left join dbo.AspNetRoles as ar
					on aur.RoleId = ar.Id
		
		where	au.AccountId = @AccountId and
				(@rolename is null OR ar.Name = @rolename) AND
				(
				(@searchTerm is null OR au.FirstName like @searchTerm + '%') OR
				(@searchTerm is null OR au.LastName like @searchTerm + '%') OR
				(@searchTerm is null OR anu.Email like '%' + @searchTerm + '%') )

	) 
	, userCounts as (
		select	count(1) as TotalCount
		from	accountUsers
	)
	select	c.TotalCount
			, u.LastName
			, u.FirstName
			, u.UserId
			, u.Email
			, u.AccountId
			, u.AccountName
			, u.RoleId
			, u.RoleName
			
	from accountUsers as u, userCounts as c

	order by u.LastName, u.FirstName, u.UserId

		offset @PageNum * @PageSize rows

		fetch next @PageSize rows only

end