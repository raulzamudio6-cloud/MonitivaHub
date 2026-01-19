
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'MessageQueue') and (COLUMN_NAME = 'Category'))) begin  
	alter table MessageQueue add Category varchar(20) null;
end;
GO

if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'MessageQueue') and (COLUMN_NAME = 'ValidUntil'))) begin  
	alter table MessageQueue add ValidUntil datetime null;
end;
GO

IF OBJECT_ID('MessageRouting', 'U') IS NULL 
BEGIN
  CREATE TABLE MessageRouting (
    id int identity,
    TransportType varchar(20),
    Category varchar(20),
	FromAddr  varchar(255),
    Channel varchar(255)
)

END

go
--insert into MessageRouting (TransportType,Category,FromAddr,Channel) values ('mail','otp',null,'notifications-email-otp')

GO


IF EXISTS(SELECT * FROM sys.types WHERE Name = 'integrations_ArrayOfEmailAttachments')
BEGIN
  if object_id('[integrations].[integrations_SendMailMessage_extended]','P') is not NULL
    drop procedure [integrations].[integrations_SendMailMessage_extended]
  if object_id('[integrations].[integrations_SendMailMessage]','P') is not NULL
    drop procedure [integrations].[integrations_SendMailMessage]
  drop type integrations.integrations_ArrayOfEmailAttachments
END
IF EXISTS(SELECT * FROM sys.types WHERE Name = 'integrations_ArrayOfEmailAddresses')
BEGIN
  if object_id('[integrations].[integrations_SendMailMessage_extended]','P') is not NULL
    drop procedure [integrations].[integrations_SendMailMessage_extended]
  if object_id('[integrations].[integrations_SendMailMessage]','P') is not NULL
    drop procedure [integrations].[integrations_SendMailMessage]
  drop type integrations.integrations_ArrayOfEmailAddresses
END




CREATE TYPE integrations.integrations_ArrayOfEmailAttachments AS TABLE
(
  Name nvarchar(255),
  MediaType nvarchar(255),
  ContentId nvarchar(255),
  IsInline nvarchar(255),
  Content varbinary(max)
)



CREATE TYPE integrations.integrations_ArrayOfEmailAddresses AS TABLE
(
  [address] nvarchar(255),
  [displayName] nvarchar(255),
  [displayNameEnconding] nvarchar(255)
)



go



CREATE OR ALTER PROCEDURE [integrations].[integrations_SendSMSMessage]
   @MsgQID int,
   @Catrgory varchar(255),
   @ValidUntil datetime,
   @toaddr nvarchar(255),
   @message nvarchar(max) -- variable types taken from:   select id, toaddr, message from MessageQueue where transporttype=1 and status=0
WITH EXECUTE AS CALLER
AS
begin

  if @ValidUntil is null set @ValidUntil = getdate()+1

  declare @MbTaskParams [nvarchar](max) =
  (
  SELECT 
    @MsgQID as 'message_queue_id'
  FOR JSON PATH, WITHOUT_ARRAY_WRAPPER 
  )

  declare @MbTaskAttachments [nvarchar](max) =
  (
  select 
   'to' = @toaddr,
   'message' = @message
  FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER 
  )

  INSERT INTO integrations.[MbTasks]
           (
            MbTaskType
           ,MbTaskCode
           ,MbTaskParams
		   ,MbTaskAttachments
           ,MbSynchronousFlag
           ,CreatedBy
		   ,ValidUntil
       )
   VALUES 
     ('IN-TRANSACTION-REQUEST',
     'notifications-sms',
     @MbTaskParams,
	 cast(@MbTaskAttachments as varbinary(max)),
     0, -- =false. Meaning: task is Asynchronous
     'integrations.integrations_SendSMSMessage',
	 @ValidUntil)

   return 0
end



GO

CREATE OR ALTER PROCEDURE [dbo].[sp_SendSMSMessage]
   @MsgQID int,
   @Category varchar(20),
   @ValidUntil datetime,
   @toaddr nvarchar(255),
   @message nvarchar(max)
as
BEGIN

DECLARE @RC int

-- TODO: Set parameter values here.

EXECUTE @RC = integrations.integrations_SendSMSMessage @MsgQID, @Category, @ValidUntil, @toaddr, @message

return @RC    

END


GO



create or alter PROCEDURE [integrations].[integrations_SendMailMessage_extended]
   @MsgQID int,
   @Category varchar(20),
   @ValidUntil datetime,
   @EMailFrom            integrations_ArrayOfEmailAddresses READONLY,
   @EmailTo              integrations_ArrayOfEmailAddresses READONLY,
   @EMailCC              integrations_ArrayOfEmailAddresses READONLY,
   @EMailBCC             integrations_ArrayOfEmailAddresses READONLY,
   @EMailSubject         nvarchar(4000),
   @EMailSubjectEncoding nvarchar(1024)='utf-8',
   @EMailBody            nvarchar(max),
   @EMailBodyEnconding   nvarchar(1024)='utf-8',
   @EMailPriority        nvarchar(1024) = 'NORMAL',
   @EMailIsBodyHtml      bit = 1,
   @Attachments          integrations.integrations_ArrayOfEmailAttachments READONLY
WITH EXECUTE AS CALLER
AS
begin

SET NOCOUNT ON

/*if ((select count(*) from @EMailFrom)<>1)
   THROW 70001,'Fatal. Several rows in EMailFrom parameter.', 1;
*/

     declare @Channel varchar(255), @FromAddrRoute nvarchar(255)
     select @Channel = Channel,  @FromAddrRoute = FromAddr from MessageRouting where TransportType = 'mail' and isnull(Category,'') = isnull(@Category,'')
	 if @Channel is null set @Channel = 'notifications-email'
	 if @FromAddrRoute = '' set @FromAddrRoute = null
  
     if  @ValidUntil is null set @ValidUntil = getdate()+7


declare @MbTaskAttachments [nvarchar](max) =
(
SELECT 
   --EMailFrom=(select [address], [displayName], [displayNameEnconding] FROM @EMailFrom FOR JSON PATH, INCLUDE_NULL_VALUES )
   'from' = isnull((select @FromAddrRoute [address]  FOR JSON PATH),(select top 1 [address], [displayName] as 'display_name', [displayNameEnconding] as 'encoding' FROM @EMailFrom FOR JSON PATH) ) 
  --,EMailTo=  (select [address], [displayName], [displayNameEnconding] FROM @EMailTo FOR JSON PATH, INCLUDE_NULL_VALUES )
  ,'tos' = (select [address], [displayName]  as 'display_name', [displayNameEnconding] as 'encoding' FROM @EMailTo where [address] is not null FOR JSON PATH)
  --,EMailCC=  (select [address], [displayName], [displayNameEnconding] FROM @EMailCC FOR JSON PATH, INCLUDE_NULL_VALUES )
  ,'ccs' = (select [address], [displayName]  as 'display_name', [displayNameEnconding] as 'encoding' FROM @EMailCC where [address] is not null FOR JSON PATH)
  -- ,EMailBCC= (select [address], [displayName], [displayNameEnconding] FROM @EMailBCC FOR JSON PATH, INCLUDE_NULL_VALUES )
  ,'bccs' = (select [address], [displayName]  as 'display_name', [displayNameEnconding] as 'encoding' FROM @EMailBCC where [address] is not null FOR JSON PATH)
  ,'subject' = @EMailSubject
  --,EMailSubjectEncoding=@EMailSubjectEncoding
  ,'body' = @EMailBody
  --,EMailBodyEncoding=@EMailBodyEnconding
  --,EMailPriority = @EMailPriority
  --,EMailIsBodyHtml = @EMailIsBodyHtml
  ,'is_html' = @EMailIsBodyHtml
  --,EMailAttachment
  ,'attachments' = (
     select [Name] as 'name',
	    [MediaType] as 'media_type',
	    cast(case [IsInline] when 'true' then 1 else 0 end as bit) as 'is_inline',
	    [ContentId] as 'content_id',
	    cast('' as xml).value('xs:base64Binary(sql:column("Content"))', 'nvarchar(max)') as 'content' 
	 from @Attachments FOR JSON PATH, INCLUDE_NULL_VALUES )
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER 
)

declare @MbTaskParams [nvarchar](max) =
(
SELECT 
   @MsgQID as 'message_queue_id'
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER 
)



     INSERT INTO integrations.[MbTasks]
           (
            MbTaskType
           ,MbTaskCode
           ,MbTaskParams
		   ,MbTaskAttachments
           ,MbSynchronousFlag
           ,CreatedBy
		   ,ValidUntil
       )
     VALUES 
     ('IN-TRANSACTION-REQUEST',
     @Channel,
     @MbTaskParams,
	 cast(@MbTaskAttachments as varbinary(max)),
     0, -- =false. Meaning: task is Asynchronous
     'integrations.integrations_SendMailMessage',
	 @ValidUntil)




return 0


end

GO

create or alter PROCEDURE [integrations].[integrations_SendMailMessage]
   @MsgQID int,
   @Category  varchar(20),
   @ValidUntil datetime,
   @EmailTo   nvarchar(255),
   @EMailCC   nvarchar(255),
   @EMailBCC  nvarchar(255),
   @Subject   nvarchar(255),
   @Message   nvarchar(max),
   @UseEmailTemplate bit = 0,
   @EMailFrom nvarchar(255) = NULL,
   @Attachments  integrations.integrations_ArrayOfEmailAttachments READONLY
WITH EXECUTE AS CALLER
AS
begin

SET NOCOUNT ON

IF EXISTS ( SELECT  1 FROM    Information_schema.Routines WHERE   Specific_schema = 'dbo' AND specific_name = 'fn_GetSettingValueStr' AND Routine_Type = 'FUNCTION' )
 select @EMailFrom = coalesce(coalesce(@EMailFrom, dbo.fn_GetSettingValueStr('integrationFromEMail')),  'macrobank@advapay.eu')
ELSE
 select @EMailFrom  = coalesce(@EMailFrom, 'macrobank@advapay.eu')

declare @var_EMailFrom integrations_ArrayOfEmailAddresses
declare @var_EmailTo  integrations_ArrayOfEmailAddresses
declare @var_EMailCC  integrations_ArrayOfEmailAddresses
declare @var_EMailBCC integrations_ArrayOfEmailAddresses
declare @var_Attachments integrations.integrations_ArrayOfEmailAttachments


INSERT INTO @var_Attachments SELECT * FROM @Attachments
DECLARE @RC int
declare @var_EmailBody nvarchar(max)


-- Using email template
if (@UseEmailTemplate = 1)
begin 
  INSERT INTO @var_Attachments EXECUTE integrations.integrations_SendMailMessage_ApplyTemplate @Message, @var_EmailBody OUT
end
ELSE
  SELECT @var_EmailBody = @Message


-- SELECT * FROM @var_Attachments 

IF (@RC <> 0) 
BEGIN
  PRINT 'Fatal: Error applying email template. Exiting function.'
  RETURN @RC
END 

/*insert into @var_EMailFrom values (@EMailFrom,null,'utf-8')
insert into @var_EmailTo  values (@EmailTo ,null,'utf-8')
insert into @var_EMailCC  values (@EMailCC ,null,'utf-8')
insert into @var_EMailBCC values (@EMailBCC,null,'utf-8') */

insert into @var_EMailFrom values (@EMailFrom, null, null)
insert into @var_EmailTo  values (@EmailTo, null, null)
insert into @var_EMailCC  values (@EMailCC, null, null)
insert into @var_EMailBCC values (@EMailBCC, null, null) 

declare @EMailIsBodyHtml bit = 0

if charindex('<html' , @var_EmailBody,0) > 0 
  set @EMailIsBodyHtml = 1

exec @RC = integrations.integrations_SendMailMessage_extended
@MsgQID = @MsgQID,
@Category = @Category,
@ValidUntil = @ValidUntil,
@EMailFrom            =  @var_EMailFrom,
@EmailTo              =  @var_EmailTo,
@EMailCC              =  @var_EMailCC,
@EMailBCC             =  @var_EMailBCC,
@EMailSubject         =  @Subject,
@EMailSubjectEncoding =  'utf-8',
@EMailBody            =  @var_EmailBody,
@EMailBodyEnconding   =  'utf-8',
@EMailPriority        =  'NORMAL',
@EMailIsBodyHtml      =  @EMailIsBodyHtml,
@Attachments          =  @var_Attachments


return @RC

end



GO



ALTER   PROCEDURE [dbo].[sp_SendMailMessage]
   @MsgQID int = 0, -- ID from table 
   @Category varchar(20),
   @ValidUntil datetime,
   @EmailTo   nvarchar(max),
   @EMailCC   nvarchar(max),
   @EMailBCC  nvarchar(max),
   @Subject   nvarchar(255),
   @Message   nvarchar(max),
   @AttachFileName nvarchar(255)=NULL
   
as
BEGIN

DECLARE @RC int

declare @attachments integrations.integrations_ArrayOfEmailAttachments

-- TODO: Set parameter values here.

EXECUTE @RC = [integrations].[integrations_SendMailMessage] 
   @MsgQID = @MsgQID
  ,@Category= @Category
  ,@ValidUntil = @ValidUntil
  ,@EmailTo = @EmailTo
  ,@EMailCC = @EMailCC
  ,@EMailBCC = @EMailBCC
  ,@Subject = @Subject
  ,@Message = @Message
  ,@UseEmailTemplate = 0
  ,@EMailFrom = null
  ,@Attachments = @attachments
  

  return @RC    

END

GO

ALTER   PROCEDURE [dbo].[spsch_SendAllMessages]
  @MQID int = null
AS
BEGIN
     declare
       @MsgID int,
       @Status smallint,
       @TheDate datetime,
	   @ValidUntil datetime,
       @Category varchar(20),
	   @ToAddr nvarchar(255),
       @EMailCC nvarchar(255),
       @EMailBCC nvarchar(255),
       @Subject nvarchar(255), 
       @Message nvarchar(max),
       @SendResult smallint,
       @TransportType int,
       @AttachFileName nvarchar(255),
       @ClientID  int,
      
       @SMTPServer nvarchar(255)
       
  
  declare rCurs cursor for
          select Id, Category, ValidUntil, ToAddr, CCAddress, BCCAddress, Subject, Message, TransportType, ClientID, AttachFileName from MessageQueue 
          where (TransportType in (0 /*EMail*/, 1 /*SMS*/)) and (Status=0) and (@MQID is null or id = @MQID) for update;
          
  open rCurs;
  
  FETCH NEXT FROM rCurs INTO @MsgID, @Category, @ValidUntil, @ToAddr, @EMailCC, @EmailBCC, @Subject, @Message, @TransportType, @ClientID, @AttachFileName;
 
  WHILE @@FETCH_STATUS = 0 /*EMail*/
  BEGIN
    set @SendResult = -1000
    if @Message <> ''
    begin
      if @TransportType = 0	-- EMail
      begin
        EXEC @SendResult = sp_SendMailMessage
		   @MsgQID = @MsgID,
		   @Category = @Category,
		   @ValidUntil = @ValidUntil,
           @EmailTo =  @ToAddr,
           @EMailCC = @EmailCC,
           @EMailBCC= @EMailBCC,
           @Subject =  @Subject,
           @Message =  @Message,
           @AttachFileName = @AttachFileName
         
        if @sendResult=0 and exists(select * from PromotionsLog where MessageID=@MsgID)
          update PromotionsLog
            set StatusFlagID=2 /*Sent*/
          where MessageID=@MsgID
        end
      else
        if @TransportType = 1	-- SMS
          EXEC @SendResult = sp_SendSMSMessage 
		   @MsgQID = @MsgID,
		   @Category = @Category,
		   @ValidUntil = @ValidUntil,
           @ToAddr =  @ToAddr,
           @Message = @Message
    end
    
    --INternal mail will be send immediately     
/*    else
      if @TransportType=2 --Internal
      begin
      
        EXEC @SendResult=sp_SendInternalMessage @ClientID, @Subject, @Message 
        if exists(select * from PromotionsLog where MessageID=@MsgID)
          if @sendResult=0 
            update PromotionsLog
              set StatusFlagID=2 --Sent
            where MessageID=@MsgID
          else
            update PromotionsLog
              set StatusFlagID=7 --No internet users
            where MessageID=@MsgID
      end
   */ 
    if @SendResult<>0 
       set @Status = 2
    else
       set @Status = 3;
       
    update MessageQueue set Status = @Status, StatusStr = @SendResult, StatusUpdated = current_timestamp where current of rCurs;                      
  
    FETCH NEXT FROM rCurs INTO @MsgID, @Category, @ValidUntil, @ToAddr, @EMailCC, @EmailBCC, @Subject, @Message, @TransportType, @ClientID, @AttachFileName;
  END
  
  close rCurs;
  deallocate rCurs; 
       
END


GO


CREATE or alter  PROCEDURE integrations.MessagesHook
  @mbhookbusinessid nvarchar(max) = null ,
  @mbhookparams nvarchar(max) = null  OUTPUT ,
  @mbhookattachments varbinary(max) = null  OUTPUT ,
  @errorcode int = null  OUTPUT 
AS
BEGIN
      declare @request nvarchar(max) = cast(@mbhookattachments as nvarchar(max))
      
      declare @message_queue_id int = JSON_VALUE(@request,'$.message_queue_id')  
      declare @error_message nvarchar(max) = JSON_VALUE(@request,'$.error_message')

	  if @message_queue_id > 0
	  begin

	    if @MbHookBusinessID = 0 
	    begin
	      update MessageQueue set Status = 1, StatusStr = null, StatusUpdated = getdate() where id = @message_queue_id
	    end
	    else
	    begin
	      update MessageQueue set Status = 2, StatusStr = @error_message, StatusUpdated = getdate() where id = @message_queue_id
        end
	  end
END

GO

exec sp_GrantAll 'integrations.MessagesHook'

GO


if not exists (select * from integrations.MbHookTasksListeners where MbHookListenerCode = 'mailer')
  insert into integrations.MbHookTasksListeners (MbHookListenerCode,MbSQLProcedureName)
  values ('mailer','integrations.MessagesHook')
  
if not exists (select * from integrations.MbHookTasksListeners where MbHookListenerCode = 'sms')
  insert into integrations.MbHookTasksListeners (MbHookListenerCode,MbSQLProcedureName)
  values ('sms','integrations.MessagesHook')
  
GO
  
  
  