

DECLARE @user int
DECLARE @profile varbinary(max)
DECLARE @code nvarchar(max)
DECLARE @data nvarchar(2048)
DECLARE @res bit
DECLARE @resMsg nvarchar(2048)

select @user =1, @profile=cast('test data' as varbinary(max)), @data = 'Some test data'

DECLARE @RC int

EXECUTE @RC = [integrations].[integrations_xp_ApplyESignature] 
   @user
  ,@profile
  ,@data
  ,@code OUTPUT
  ,@res OUTPUT
  ,@resMsg OUTPUT


print '@code='+coalesce(cast(@code as nvarchar(max)), 'NULL')
print '@RC='+coalesce(cast(@RC as nvarchar(max)), 'NULL')
print 'EXECUTION RESULTS: @res='+coalesce(cast(@res as nvarchar(max)), 'NULL')+'|@resMsg='+coalesce(cast(@resMsg as nvarchar(max)), 'NULL')


go


DECLARE @user int
DECLARE @profile varbinary(max)
DECLARE @code nvarchar(max)
DECLARE @data nvarchar(2048)
DECLARE @res bit
DECLARE @resMsg nvarchar(2048)

select @user =1, @profile=cast('test data' as varbinary(max)), @data = 'Some test data', @code = '02248'

DECLARE @RC int

EXECUTE @RC = [integrations].[integrations_xp_ValidateESignature] 
   @user
  ,@profile
  ,@code
  ,@data
  ,@res OUTPUT
  ,@resMsg OUTPUT


print '@RC='+coalesce(cast(@RC as nvarchar(max)), 'NULL')
print 'EXECUTION RESULTS: @res='+coalesce(cast(@res as nvarchar(max)), 'NULL')+'|@resMsg='+coalesce(cast(@resMsg as nvarchar(max)), 'NULL')


go



