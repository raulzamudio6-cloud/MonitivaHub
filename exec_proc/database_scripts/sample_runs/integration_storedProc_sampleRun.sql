declare @RC2 int, @Result nvarchar(4000)
DECLARE @json NVARCHAR(MAX) = '{}';
SET @json = JSON_MODIFY(@json, '$.docId', 123);
exec @RC2 = integrations.sp_ExecAndCommit 'ping',@json, @Result out, 0
select @RC2, @Result