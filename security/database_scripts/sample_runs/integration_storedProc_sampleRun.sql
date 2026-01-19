declare @RemoteUserID int=1, @ClientID int=1
declare @keyblob nvarchar(max),  @aerr int, @keybuff varbinary(max), @keyt varbinary(max)
declare @res bit, @resMsg nvarchar(2048), @key varbinary(200) 

select @keyblob = 'dd8d20e2c2fc9c19eb17ff26f68f82a8e5c14dc6'                                                                                                                 

set @keybuff = CONVERT(varbinary(max), @keyblob, 2);

exec [integrations].[integrations_xp_GetOtpKey]  @ClientID, @keybuff, @res out, @resMsg out, @keyt out

set @key = @keyt;

select @keybuff
select @key

GO

DECLARE @user int
DECLARE @profile varbinary(max)
DECLARE @res bit
DECLARE @resMsg nvarchar(2048)
DECLARE @plainKey varbinary(max)



select @user = 1, @profile=cast('test data' as varbinary(max))


DECLARE @RC int

EXECUTE @RC = [integrations].[integrations_xp_GetMacKey] 
   @user
  ,@profile
  ,@res OUTPUT
  ,@resMsg OUTPUT
  ,@plainKey OUTPUT


print '@RC='+coalesce(cast(@RC as nvarchar(max)), 'NULL')
print 'EXECUTION RESULTS: @res='+coalesce(cast(@res as nvarchar(max)), 'NULL')+'|@resMsg='+coalesce(cast(@resMsg as nvarchar(max)), 'NULL')+'|@plainKey='+coalesce(cast(@plainKey as nvarchar(max)), 'NULL')







go



DECLARE @user int
DECLARE @res bit
DECLARE @resMsg nvarchar(2048)
DECLARE @plainKey nvarchar(max)

select @user = 1


DECLARE @RC int

exec @RC =[integrations].[integrations_xp_GenMacKey] 
   @user
  ,@res OUTPUT
  ,@resMsg OUTPUT
  ,@plainKey OUTPUT

print '@RC='+coalesce(cast(@RC as nvarchar(max)), 'NULL')
print 'EXECUTION RESULTS: @res='+coalesce(cast(@res as nvarchar(max)), 'NULL')+'|@resMsg='+coalesce(cast(@resMsg as nvarchar(max)), 'NULL')+'|@plainKey='+coalesce(cast(@plainKey as nvarchar(max)), 'NULL')






go




DECLARE @user int
DECLARE @profile varbinary(max)
DECLARE @code int
DECLARE @data nvarchar(2048)
DECLARE @res bit
DECLARE @resMsg nvarchar(2048)

select @user =1, @profile=cast('test data' as varbinary(max)), @code =1234

DECLARE @RC int

EXECUTE @RC = [integrations].[integrations_xp_ValidMacSign] 
   @user
  ,@profile
  ,@code
  ,@data
  ,@res OUTPUT
  ,@resMsg OUTPUT


print '@RC='+coalesce(cast(@RC as nvarchar(max)), 'NULL')
print 'EXECUTION RESULTS: @res='+coalesce(cast(@res as nvarchar(max)), 'NULL')+'|@resMsg='+coalesce(cast(@resMsg as nvarchar(max)), 'NULL')


go



DECLARE @user int
DECLARE @profile varbinary(max)
DECLARE @res bit
DECLARE @resMsg nvarchar(2048)
DECLARE @plainKey varbinary(max)



select @user = 1, @profile=cast('test data' as varbinary(max))


DECLARE @RC int

EXECUTE @RC = [integrations].[integrations_xp_GetOtpKey] 
   @user
  ,@profile
  ,@res OUTPUT
  ,@resMsg OUTPUT
  ,@plainKey OUTPUT


print '@RC='+coalesce(cast(@RC as nvarchar(max)), 'NULL')
print 'EXECUTION RESULTS: @res='+coalesce(cast(@res as nvarchar(max)), 'NULL')+'|@resMsg='+coalesce(cast(@resMsg as nvarchar(max)), 'NULL')+'|@plainKey='+coalesce(cast(@plainKey as nvarchar(max)), 'NULL')







go



DECLARE @user int
DECLARE @res bit
DECLARE @resMsg nvarchar(2048)
DECLARE @plainKey nvarchar(max)

select @user = 1


DECLARE @RC int

exec @RC =[integrations].[integrations_xp_GenOtpKey] 
   @user
  ,@res OUTPUT
  ,@resMsg OUTPUT
  ,@plainKey OUTPUT

print '@RC='+coalesce(cast(@RC as nvarchar(max)), 'NULL')
print 'EXECUTION RESULTS: @res='+coalesce(cast(@res as nvarchar(max)), 'NULL')+'|@resMsg='+coalesce(cast(@resMsg as nvarchar(max)), 'NULL')+'|@plainKey='+coalesce(cast(@plainKey as nvarchar(max)), 'NULL')




go


DECLARE @user int
DECLARE @profile varbinary(max)
DECLARE @code int
DECLARE @res bit
DECLARE @resMsg nvarchar(2048)
DECLARE @timestamp bigint


select @user = 1, @profile=cast('test data' as varbinary(max)), @code = 1234

DECLARE @RC int

EXECUTE @RC = [integrations].[integrations_xp_Authenticate] 
   @user
  ,@profile
  ,@code
  ,@res OUTPUT
  ,@resMsg OUTPUT
  ,@timestamp OUTPUT


print '@RC='+coalesce(cast(@RC as nvarchar(max)), 'NULL')
print 'EXECUTION RESULTS: @res='+coalesce(cast(@res as nvarchar(max)), 'NULL')+'|@resMsg='+coalesce(cast(@resMsg as nvarchar(max)), 'NULL')


go







