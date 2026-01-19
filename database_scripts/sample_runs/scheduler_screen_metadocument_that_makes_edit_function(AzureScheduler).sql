SET NOCOUNT ON

declare @meta_DocumentID int
declare @meta_FieldID int
declare @action nvarchar(255)
set @meta_DocumentID = null 
select @meta_DocumentID = id from meta_Documents where DocClassShortName=N'AzureScheduler' 
set @meta_DocumentID = isnull(@meta_DocumentID,0)
if @meta_DocumentID = 0 set @action = 'INS' else set @action = 'UPD'
exec @meta_DocumentID=sp_meta_Documents_CreateOrUpdate  @ID=@meta_DocumentID,@Action=@action, @DocClassShortName='AzureScheduler',@IsActive=1, @Caption_Operator=N'Azure Scheduler', @Caption_Client=N'Azure Scheduler',@TypeID=0, @MenuSectionShortName =N'SPECIALS' ,@DBViewToDisplay_Operator=NULL, @DBViewToDisplay_Client=NULL, @QueryToCheckVisibility=N'', @VisibleToOperator=1, @PictureNumber=-1, @MenuSectionPriority=NULL

declare @PageID int
declare @GroupID int
declare @AgreementID int
declare @TextTemplateID int


delete from meta_Fields where meta_DocumentID=@meta_DocumentID and meta_TypeId in (2,3) 

exec @PageID=sp_meta_Fields_GroupCU   @ID=0, @Action='INS', @meta_DocumentID=@meta_DocumentID,
    @OwnerID=NULL, 
    @Name=N'main_page',
    @Caption=NULL,
    @Hint=NULL,
    @ParentFields=N'',
    @Tag=NULL

PRINT 'Page "'+N'<No caption specified>'+'" created'

exec @GroupID=sp_meta_Fields_GroupCU   @ID=0, @Action='INS', @meta_DocumentID=@meta_DocumentID,
    @OwnerID=@PageID, 
    @Name=N'general_group',
    @Caption=NULL,
    @Hint=NULL,
    @ParentFields=N'',
    @Tag=NULL

PRINT '  Group "'+N'<No caption specified>'+'" created'

set @AgreementID=NULL
set @TextTemplateID=NULL
set @meta_FieldID = null
select @meta_FieldID = id from meta_Fields where meta_DocumentID=@meta_DocumentID and FieldName=N'Name' 
set @meta_FieldID = isnull(@meta_FieldID,0)
if @meta_FieldID = 0 set @action = 'INS' else set @action = 'UPD'
exec @meta_FieldID=sp_meta_Fields_CreateOrUpdate @ID=@meta_FieldID, @Action=@action, @meta_DocumentID=@meta_DocumentID,
            @meta_typeID=4,
            @OwnerID=@GroupID, 
            @FieldName=N'Name',
            @TableCaption_Operator=N'Name',
            @TableCaption_Client=N'Name',
            @FormCaption_Operator=N'Name',
            @FormCaption_Client=N'Name',
            @FormHint_Operator=NULL,
            @FormHint_Client=NULL,
            @IsFromBaseTable =0,
            @IsRequired      =1,
            @IsCalculated    =0,
            @IsReadOnly      =0,
            @IsVirtual       =0,
            @DefaultValue      =NULL,
            @DefaultValueQuery =N'',
            @CalculationSQL    =N'',
            @VisibleInTable_Operator =1,
            @VisibleInTable_Client   =1,
            @VisibleOnForm_Operator  =1,
            @VisibleOnForm_Client    =1,
            @IncludeInTableSelect    =0,
            @ParentFields       =N'',
            @RegExp             =NULL,
            @RegExpErrorMessage =NULL,
            @CurrencyList_UseProfile   =1,
            @CurrencyList_SellCurrency =0,
            @DictionaryQuery           =N'',
            @DictionaryValueQuery      =N'',
            @DictionaryEmptyLine       =NULL,
            @Account_ClientOnly        =1,
            @Account_BalanceAccounts   =N'',
            @AgreementID        =@AgreementID,
            @TextTemplateID     =@TextTemplateID,
            @Tag                =NULL

PRINT '      Field "'+N'Name'+'" '+case when @action = 'INS' then 'created' else 'updated' end

delete from meta_DocHandbooks_Fields where meta_FieldID = @meta_FieldID
set @AgreementID=NULL
set @TextTemplateID=NULL
set @meta_FieldID = null
select @meta_FieldID = id from meta_Fields where meta_DocumentID=@meta_DocumentID and FieldName=N'Code' 
set @meta_FieldID = isnull(@meta_FieldID,0)
if @meta_FieldID = 0 set @action = 'INS' else set @action = 'UPD'
exec @meta_FieldID=sp_meta_Fields_CreateOrUpdate @ID=@meta_FieldID, @Action=@action, @meta_DocumentID=@meta_DocumentID,
            @meta_typeID=4,
            @OwnerID=@GroupID, 
            @FieldName=N'Code',
            @TableCaption_Operator=N'Code',
            @TableCaption_Client=N'Code',
            @FormCaption_Operator=N'Code',
            @FormCaption_Client=N'Code',
            @FormHint_Operator=NULL,
            @FormHint_Client=NULL,
            @IsFromBaseTable =0,
            @IsRequired      =0,
            @IsCalculated    =0,
            @IsReadOnly      =0,
            @IsVirtual       =0,
            @DefaultValue      =NULL,
            @DefaultValueQuery =N'',
            @CalculationSQL    =N'',
            @VisibleInTable_Operator =1,
            @VisibleInTable_Client   =1,
            @VisibleOnForm_Operator  =1,
            @VisibleOnForm_Client    =1,
            @IncludeInTableSelect    =0,
            @ParentFields       =N'',
            @RegExp             =NULL,
            @RegExpErrorMessage =NULL,
            @CurrencyList_UseProfile   =1,
            @CurrencyList_SellCurrency =0,
            @DictionaryQuery           =N'',
            @DictionaryValueQuery      =N'',
            @DictionaryEmptyLine       =NULL,
            @Account_ClientOnly        =1,
            @Account_BalanceAccounts   =N'',
            @AgreementID        =@AgreementID,
            @TextTemplateID     =@TextTemplateID,
            @Tag                =N'web-width:4'

PRINT '      Field "'+N'Code'+'" '+case when @action = 'INS' then 'created' else 'updated' end

delete from meta_DocHandbooks_Fields where meta_FieldID = @meta_FieldID
set @AgreementID=NULL
set @TextTemplateID=NULL
set @meta_FieldID = null
select @meta_FieldID = id from meta_Fields where meta_DocumentID=@meta_DocumentID and FieldName=N'Enabled' 
set @meta_FieldID = isnull(@meta_FieldID,0)
if @meta_FieldID = 0 set @action = 'INS' else set @action = 'UPD'
exec @meta_FieldID=sp_meta_Fields_CreateOrUpdate @ID=@meta_FieldID, @Action=@action, @meta_DocumentID=@meta_DocumentID,
            @meta_typeID=10,
            @OwnerID=@GroupID, 
            @FieldName=N'Enabled',
            @TableCaption_Operator=N'Enabled',
            @TableCaption_Client=N'Enabled',
            @FormCaption_Operator=N'Enabled',
            @FormCaption_Client=N'Enabled',
            @FormHint_Operator=NULL,
            @FormHint_Client=NULL,
            @IsFromBaseTable =0,
            @IsRequired      =0,
            @IsCalculated    =0,
            @IsReadOnly      =0,
            @IsVirtual       =0,
            @DefaultValue      =N'1',
            @DefaultValueQuery =N'',
            @CalculationSQL    =N'',
            @VisibleInTable_Operator =0,
            @VisibleInTable_Client   =0,
            @VisibleOnForm_Operator  =0,
            @VisibleOnForm_Client    =0,
            @IncludeInTableSelect    =0,
            @ParentFields       =N'',
            @RegExp             =NULL,
            @RegExpErrorMessage =NULL,
            @CurrencyList_UseProfile   =1,
            @CurrencyList_SellCurrency =0,
            @DictionaryQuery           =N'',
            @DictionaryValueQuery      =N'',
            @DictionaryEmptyLine       =NULL,
            @Account_ClientOnly        =1,
            @Account_BalanceAccounts   =N'',
            @AgreementID        =@AgreementID,
            @TextTemplateID     =@TextTemplateID,
            @Tag                =NULL

PRINT '      Field "'+N'Enabled'+'" '+case when @action = 'INS' then 'created' else 'updated' end

delete from meta_DocHandbooks_Fields where meta_FieldID = @meta_FieldID
set @AgreementID=NULL
set @TextTemplateID=NULL
set @meta_FieldID = null
select @meta_FieldID = id from meta_Fields where meta_DocumentID=@meta_DocumentID and FieldName=N'MbSchedulerID' 
set @meta_FieldID = isnull(@meta_FieldID,0)
if @meta_FieldID = 0 set @action = 'INS' else set @action = 'UPD'
exec @meta_FieldID=sp_meta_Fields_CreateOrUpdate @ID=@meta_FieldID, @Action=@action, @meta_DocumentID=@meta_DocumentID,
            @meta_typeID=6,
            @OwnerID=@GroupID, 
            @FieldName=N'MbSchedulerID',
            @TableCaption_Operator=N'MbSchedulerID',
            @TableCaption_Client=N'MbSchedulerID',
            @FormCaption_Operator=N'MbSchedulerID',
            @FormCaption_Client=N'MbSchedulerID',
            @FormHint_Operator=NULL,
            @FormHint_Client=NULL,
            @IsFromBaseTable =0,
            @IsRequired      =0,
            @IsCalculated    =0,
            @IsReadOnly      =1,
            @IsVirtual       =0,
            @DefaultValue      =NULL,
            @DefaultValueQuery =N'',
            @CalculationSQL    =N'',
            @VisibleInTable_Operator =1,
            @VisibleInTable_Client   =1,
            @VisibleOnForm_Operator  =1,
            @VisibleOnForm_Client    =1,
            @IncludeInTableSelect    =0,
            @ParentFields       =N'',
            @RegExp             =NULL,
            @RegExpErrorMessage =NULL,
            @CurrencyList_UseProfile   =1,
            @CurrencyList_SellCurrency =0,
            @DictionaryQuery           =N'',
            @DictionaryValueQuery      =N'',
            @DictionaryEmptyLine       =NULL,
            @Account_ClientOnly        =1,
            @Account_BalanceAccounts   =N'',
            @AgreementID        =@AgreementID,
            @TextTemplateID     =@TextTemplateID,
            @Tag                =NULL

PRINT '      Field "'+N'MbSchedulerID'+'" '+case when @action = 'INS' then 'created' else 'updated' end

delete from meta_DocHandbooks_Fields where meta_FieldID = @meta_FieldID
exec @GroupID=sp_meta_Fields_GroupCU   @ID=0, @Action='INS', @meta_DocumentID=@meta_DocumentID,
    @OwnerID=@PageID, 
    @Name=N'schedule_group',
    @Caption=N'Schedule',
    @Hint=NULL,
    @ParentFields=N'',
    @Tag=NULL

PRINT '  Group "'+N'Schedule'+'" created'

set @AgreementID=NULL
set @TextTemplateID=NULL
set @meta_FieldID = null
select @meta_FieldID = id from meta_Fields where meta_DocumentID=@meta_DocumentID and FieldName=N'StartDate' 
set @meta_FieldID = isnull(@meta_FieldID,0)
if @meta_FieldID = 0 set @action = 'INS' else set @action = 'UPD'
exec @meta_FieldID=sp_meta_Fields_CreateOrUpdate @ID=@meta_FieldID, @Action=@action, @meta_DocumentID=@meta_DocumentID,
            @meta_typeID=9,
            @OwnerID=@GroupID, 
            @FieldName=N'StartDate',
            @TableCaption_Operator=N'Start Date',
            @TableCaption_Client=N'Start Date',
            @FormCaption_Operator=N'Start Date',
            @FormCaption_Client=N'Start Date',
            @FormHint_Operator=NULL,
            @FormHint_Client=NULL,
            @IsFromBaseTable =0,
            @IsRequired      =1,
            @IsCalculated    =1,
            @IsReadOnly      =0,
            @IsVirtual       =0,
            @DefaultValue      =NULL,
            @DefaultValueQuery =N'select getdate()




',
            @CalculationSQL    =N'select ''scheduler_group'' fieldname, case when @enabled = 1 then 0 else 1 end readonly
set @enddate = getdate()+ 3000
set @FrequencyType = 1',
            @VisibleInTable_Operator =1,
            @VisibleInTable_Client   =1,
            @VisibleOnForm_Operator  =1,
            @VisibleOnForm_Client    =1,
            @IncludeInTableSelect    =0,
            @ParentFields       =N'<ParentFields>
  <Field Name="Enabled" />

</ParentFields>',
            @RegExp             =NULL,
            @RegExpErrorMessage =NULL,
            @CurrencyList_UseProfile   =1,
            @CurrencyList_SellCurrency =0,
            @DictionaryQuery           =N'',
            @DictionaryValueQuery      =N'',
            @DictionaryEmptyLine       =NULL,
            @Account_ClientOnly        =1,
            @Account_BalanceAccounts   =N'',
            @AgreementID        =@AgreementID,
            @TextTemplateID     =@TextTemplateID,
            @Tag                =N'web-label-width:3 web-width:3'

PRINT '      Field "'+N'Start Date'+'" '+case when @action = 'INS' then 'created' else 'updated' end

delete from meta_DocHandbooks_Fields where meta_FieldID = @meta_FieldID
set @AgreementID=NULL
set @TextTemplateID=NULL
set @meta_FieldID = null
select @meta_FieldID = id from meta_Fields where meta_DocumentID=@meta_DocumentID and FieldName=N'EndDate' 
set @meta_FieldID = isnull(@meta_FieldID,0)
if @meta_FieldID = 0 set @action = 'INS' else set @action = 'UPD'
exec @meta_FieldID=sp_meta_Fields_CreateOrUpdate @ID=@meta_FieldID, @Action=@action, @meta_DocumentID=@meta_DocumentID,
            @meta_typeID=9,
            @OwnerID=@GroupID, 
            @FieldName=N'EndDate',
            @TableCaption_Operator=N'End Date',
            @TableCaption_Client=N'End Date',
            @FormCaption_Operator=N'End Date',
            @FormCaption_Client=N'End Date',
            @FormHint_Operator=NULL,
            @FormHint_Client=NULL,
            @IsFromBaseTable =0,
            @IsRequired      =1,
            @IsCalculated    =0,
            @IsReadOnly      =0,
            @IsVirtual       =0,
            @DefaultValue      =N'99991231',
            @DefaultValueQuery =N'

',
            @CalculationSQL    =N'',
            @VisibleInTable_Operator =1,
            @VisibleInTable_Client   =1,
            @VisibleOnForm_Operator  =1,
            @VisibleOnForm_Client    =1,
            @IncludeInTableSelect    =0,
            @ParentFields       =N'',
            @RegExp             =NULL,
            @RegExpErrorMessage =NULL,
            @CurrencyList_UseProfile   =1,
            @CurrencyList_SellCurrency =0,
            @DictionaryQuery           =N'',
            @DictionaryValueQuery      =N'',
            @DictionaryEmptyLine       =NULL,
            @Account_ClientOnly        =1,
            @Account_BalanceAccounts   =N'',
            @AgreementID        =@AgreementID,
            @TextTemplateID     =@TextTemplateID,
            @Tag                =N'web-line:same web-label-width:2 web-width:3'

PRINT '      Field "'+N'End Date'+'" '+case when @action = 'INS' then 'created' else 'updated' end

delete from meta_DocHandbooks_Fields where meta_FieldID = @meta_FieldID
set @AgreementID=NULL
set @TextTemplateID=NULL
set @meta_FieldID = null
select @meta_FieldID = id from meta_Fields where meta_DocumentID=@meta_DocumentID and FieldName=N'StartTime' 
set @meta_FieldID = isnull(@meta_FieldID,0)
if @meta_FieldID = 0 set @action = 'INS' else set @action = 'UPD'
exec @meta_FieldID=sp_meta_Fields_CreateOrUpdate @ID=@meta_FieldID, @Action=@action, @meta_DocumentID=@meta_DocumentID,
            @meta_typeID=4,
            @OwnerID=@GroupID, 
            @FieldName=N'StartTime',
            @TableCaption_Operator=N'Start Time',
            @TableCaption_Client=N'Start Time',
            @FormCaption_Operator=N'Start Time',
            @FormCaption_Client=N'Start Time',
            @FormHint_Operator=NULL,
            @FormHint_Client=NULL,
            @IsFromBaseTable =0,
            @IsRequired      =1,
            @IsCalculated    =0,
            @IsReadOnly      =0,
            @IsVirtual       =0,
            @DefaultValue      =N'000000',
            @DefaultValueQuery =N'',
            @CalculationSQL    =N'',
            @VisibleInTable_Operator =1,
            @VisibleInTable_Client   =1,
            @VisibleOnForm_Operator  =1,
            @VisibleOnForm_Client    =1,
            @IncludeInTableSelect    =0,
            @ParentFields       =N'',
            @RegExp             =N'^\d{6}$',
            @RegExpErrorMessage =N'Format HHMMSS (6 digits)',
            @CurrencyList_UseProfile   =1,
            @CurrencyList_SellCurrency =0,
            @DictionaryQuery           =N'',
            @DictionaryValueQuery      =N'',
            @DictionaryEmptyLine       =NULL,
            @Account_ClientOnly        =1,
            @Account_BalanceAccounts   =N'',
            @AgreementID        =@AgreementID,
            @TextTemplateID     =@TextTemplateID,
            @Tag                =N'web-label-width:3 web-width:2'

PRINT '      Field "'+N'Start Time'+'" '+case when @action = 'INS' then 'created' else 'updated' end

delete from meta_DocHandbooks_Fields where meta_FieldID = @meta_FieldID
set @AgreementID=NULL
set @TextTemplateID=NULL
set @meta_FieldID = null
select @meta_FieldID = id from meta_Fields where meta_DocumentID=@meta_DocumentID and FieldName=N'EndTime' 
set @meta_FieldID = isnull(@meta_FieldID,0)
if @meta_FieldID = 0 set @action = 'INS' else set @action = 'UPD'
exec @meta_FieldID=sp_meta_Fields_CreateOrUpdate @ID=@meta_FieldID, @Action=@action, @meta_DocumentID=@meta_DocumentID,
            @meta_typeID=4,
            @OwnerID=@GroupID, 
            @FieldName=N'EndTime',
            @TableCaption_Operator=N'End Time',
            @TableCaption_Client=N'End Time',
            @FormCaption_Operator=N'End Time',
            @FormCaption_Client=N'End Time',
            @FormHint_Operator=NULL,
            @FormHint_Client=NULL,
            @IsFromBaseTable =0,
            @IsRequired      =1,
            @IsCalculated    =0,
            @IsReadOnly      =0,
            @IsVirtual       =0,
            @DefaultValue      =N'235959',
            @DefaultValueQuery =N'',
            @CalculationSQL    =N'',
            @VisibleInTable_Operator =1,
            @VisibleInTable_Client   =1,
            @VisibleOnForm_Operator  =1,
            @VisibleOnForm_Client    =1,
            @IncludeInTableSelect    =0,
            @ParentFields       =N'',
            @RegExp             =N'^\d{6}$',
            @RegExpErrorMessage =N'Format HHMMSS (6 digits)',
            @CurrencyList_UseProfile   =1,
            @CurrencyList_SellCurrency =0,
            @DictionaryQuery           =N'',
            @DictionaryValueQuery      =N'',
            @DictionaryEmptyLine       =NULL,
            @Account_ClientOnly        =1,
            @Account_BalanceAccounts   =N'',
            @AgreementID        =@AgreementID,
            @TextTemplateID     =@TextTemplateID,
            @Tag                =N'web-line:same web-before-label:1 web-label-width:2 web-width:2'

PRINT '      Field "'+N'End Time'+'" '+case when @action = 'INS' then 'created' else 'updated' end

delete from meta_DocHandbooks_Fields where meta_FieldID = @meta_FieldID
set @AgreementID=NULL
set @TextTemplateID=NULL
set @meta_FieldID = null
select @meta_FieldID = id from meta_Fields where meta_DocumentID=@meta_DocumentID and FieldName=N'FrequencyType' 
set @meta_FieldID = isnull(@meta_FieldID,0)
if @meta_FieldID = 0 set @action = 'INS' else set @action = 'UPD'
exec @meta_FieldID=sp_meta_Fields_CreateOrUpdate @ID=@meta_FieldID, @Action=@action, @meta_DocumentID=@meta_DocumentID,
            @meta_typeID=11,
            @OwnerID=@GroupID, 
            @FieldName=N'FrequencyType',
            @TableCaption_Operator=N'Frequency Type',
            @TableCaption_Client=N'Frequency Type',
            @FormCaption_Operator=N'Frequency Type',
            @FormCaption_Client=N'Frequency Type',
            @FormHint_Operator=NULL,
            @FormHint_Client=NULL,
            @IsFromBaseTable =0,
            @IsRequired      =1,
            @IsCalculated    =0,
            @IsReadOnly      =0,
            @IsVirtual       =0,
            @DefaultValue      =N'Every few seconds',
            @DefaultValueQuery =N'
',
            @CalculationSQL    =N'',
            @VisibleInTable_Operator =1,
            @VisibleInTable_Client   =1,
            @VisibleOnForm_Operator  =1,
            @VisibleOnForm_Client    =1,
            @IncludeInTableSelect    =0,
            @ParentFields       =N'',
            @RegExp             =NULL,
            @RegExpErrorMessage =NULL,
            @CurrencyList_UseProfile   =1,
            @CurrencyList_SellCurrency =0,
            @DictionaryQuery           =N'select 1 #ID, ''Every few seconds'' Description
union
select 2 #ID, ''Once a day'' Description 




',
            @DictionaryValueQuery      =N'case when :id=1 then ''Every few seconds''
     when :id=2 then ''Once a day''     
     else ''???''
end




',
            @DictionaryEmptyLine       =NULL,
            @Account_ClientOnly        =1,
            @Account_BalanceAccounts   =N'',
            @AgreementID        =@AgreementID,
            @TextTemplateID     =@TextTemplateID,
            @Tag                =N'web-label-width:3 web-width:4'

PRINT '      Field "'+N'Frequency Type'+'" '+case when @action = 'INS' then 'created' else 'updated' end

delete from meta_DocHandbooks_Fields where meta_FieldID = @meta_FieldID
set @AgreementID=NULL
set @TextTemplateID=NULL
set @meta_FieldID = null
select @meta_FieldID = id from meta_Fields where meta_DocumentID=@meta_DocumentID and FieldName=N'IntervalInSeconds' 
set @meta_FieldID = isnull(@meta_FieldID,0)
if @meta_FieldID = 0 set @action = 'INS' else set @action = 'UPD'
exec @meta_FieldID=sp_meta_Fields_CreateOrUpdate @ID=@meta_FieldID, @Action=@action, @meta_DocumentID=@meta_DocumentID,
            @meta_typeID=6,
            @OwnerID=@GroupID, 
            @FieldName=N'IntervalInSeconds',
            @TableCaption_Operator=N'Interval In Seconds',
            @TableCaption_Client=N'Interval In Seconds',
            @FormCaption_Operator=N'Interval In Seconds',
            @FormCaption_Client=N'Interval In Seconds',
            @FormHint_Operator=NULL,
            @FormHint_Client=NULL,
            @IsFromBaseTable =0,
            @IsRequired      =1,
            @IsCalculated    =0,
            @IsReadOnly      =0,
            @IsVirtual       =0,
            @DefaultValue      =N'1',
            @DefaultValueQuery =N'',
            @CalculationSQL    =N'',
            @VisibleInTable_Operator =1,
            @VisibleInTable_Client   =1,
            @VisibleOnForm_Operator  =1,
            @VisibleOnForm_Client    =1,
            @IncludeInTableSelect    =0,
            @ParentFields       =N'<ParentFields>
  <Field Name="FrequencyType">
  <Visibility DefaultVisibilityFlag="False">
    <ParentItem ID="1" Value="Every few seconds" Visible="True" />
    <ParentItem ID="2" Value="Once a day" Visible="False" />
  </Visibility>
</Field>

</ParentFields>',
            @RegExp             =NULL,
            @RegExpErrorMessage =NULL,
            @CurrencyList_UseProfile   =1,
            @CurrencyList_SellCurrency =0,
            @DictionaryQuery           =N'',
            @DictionaryValueQuery      =N'',
            @DictionaryEmptyLine       =NULL,
            @Account_ClientOnly        =1,
            @Account_BalanceAccounts   =N'',
            @AgreementID        =@AgreementID,
            @TextTemplateID     =@TextTemplateID,
            @Tag                =N'web-line:same web-label-width:3 web-width:4'

PRINT '      Field "'+N'Interval In Seconds'+'" '+case when @action = 'INS' then 'created' else 'updated' end

delete from meta_DocHandbooks_Fields where meta_FieldID = @meta_FieldID
exec @GroupID=sp_meta_Fields_GroupCU   @ID=0, @Action='INS', @meta_DocumentID=@meta_DocumentID,
    @OwnerID=@PageID, 
    @Name=N'command_group',
    @Caption=N'Command',
    @Hint=NULL,
    @ParentFields=N'',
    @Tag=NULL

PRINT '  Group "'+N'Command'+'" created'

set @AgreementID=NULL
set @TextTemplateID=NULL
set @meta_FieldID = null
select @meta_FieldID = id from meta_Fields where meta_DocumentID=@meta_DocumentID and FieldName=N'Command' 
set @meta_FieldID = isnull(@meta_FieldID,0)
if @meta_FieldID = 0 set @action = 'INS' else set @action = 'UPD'
exec @meta_FieldID=sp_meta_Fields_CreateOrUpdate @ID=@meta_FieldID, @Action=@action, @meta_DocumentID=@meta_DocumentID,
            @meta_typeID=5,
            @OwnerID=@GroupID, 
            @FieldName=N'Command',
            @TableCaption_Operator=N'Command',
            @TableCaption_Client=N'Command',
            @FormCaption_Operator=N'Command',
            @FormCaption_Client=N'Command',
            @FormHint_Operator=NULL,
            @FormHint_Client=NULL,
            @IsFromBaseTable =0,
            @IsRequired      =1,
            @IsCalculated    =0,
            @IsReadOnly      =0,
            @IsVirtual       =0,
            @DefaultValue      =NULL,
            @DefaultValueQuery =N'',
            @CalculationSQL    =N'',
            @VisibleInTable_Operator =1,
            @VisibleInTable_Client   =1,
            @VisibleOnForm_Operator  =1,
            @VisibleOnForm_Client    =1,
            @IncludeInTableSelect    =0,
            @ParentFields       =N'',
            @RegExp             =NULL,
            @RegExpErrorMessage =NULL,
            @CurrencyList_UseProfile   =1,
            @CurrencyList_SellCurrency =0,
            @DictionaryQuery           =N'',
            @DictionaryValueQuery      =N'',
            @DictionaryEmptyLine       =NULL,
            @Account_ClientOnly        =1,
            @Account_BalanceAccounts   =N'',
            @AgreementID        =@AgreementID,
            @TextTemplateID     =@TextTemplateID,
            @Tag                =N'web-label:hide web-width:12 web-rowspan:5'

PRINT '      Field "'+N'Command'+'" '+case when @action = 'INS' then 'created' else 'updated' end

delete from meta_DocHandbooks_Fields where meta_FieldID = @meta_FieldID


if not exists (select * from dbo.sysobjects where id = object_id('DocDataAzureScheduler') ) 
begin
  CREATE TABLE DocDataAzureScheduler (
    ID int NOT NULL,
    CONSTRAINT PK_DocDataAzureScheduler PRIMARY KEY (ID)
  )
  print ' table DocDataAzureScheduler created' 
end

if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler') and (COLUMN_NAME = 'Name'))) begin  
     alter table DocDataAzureScheduler add    Name nvarchar(255) NULL
 print ' column Name added' 
end 
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler') and (COLUMN_NAME = 'Code'))) begin  
     alter table DocDataAzureScheduler add    Code nvarchar(255) NULL
 print ' column Code added' 
end 
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler') and (COLUMN_NAME = 'Enabled'))) begin  
     alter table DocDataAzureScheduler add    Enabled bit  NULL
 print ' column Enabled added' 
end 
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler') and (COLUMN_NAME = 'MbSchedulerID'))) begin  
     alter table DocDataAzureScheduler add    MbSchedulerID int NULL
 print ' column MbSchedulerID added' 
end 
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler') and (COLUMN_NAME = 'StartDate'))) begin  
     alter table DocDataAzureScheduler add    StartDate datetime  NULL
 print ' column StartDate added' 
end 
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler') and (COLUMN_NAME = 'EndDate'))) begin  
     alter table DocDataAzureScheduler add    EndDate datetime  NULL
 print ' column EndDate added' 
end 
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler') and (COLUMN_NAME = 'StartTime'))) begin  
     alter table DocDataAzureScheduler add    StartTime nvarchar(255) NULL
 print ' column StartTime added' 
end 
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler') and (COLUMN_NAME = 'EndTime'))) begin  
     alter table DocDataAzureScheduler add    EndTime nvarchar(255) NULL
 print ' column EndTime added' 
end 
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler') and (COLUMN_NAME = 'FrequencyType'))) begin  
     alter table DocDataAzureScheduler add    FrequencyType int NULL
 print ' column FrequencyType added' 
end 
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler') and (COLUMN_NAME = 'IntervalInSeconds'))) begin  
     alter table DocDataAzureScheduler add    IntervalInSeconds int NULL
 print ' column IntervalInSeconds added' 
end 
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler') and (COLUMN_NAME = 'Command'))) begin  
     alter table DocDataAzureScheduler add    Command nvarchar(max) NULL
 print ' column Command added' 
end 




if not exists (select * from dbo.sysobjects where id = object_id('DocDataAzureScheduler_Versions') ) 
begin
  CREATE TABLE DocDataAzureScheduler_Versions (
    ID int NOT NULL,
    Version int,
    CONSTRAINT PK_DocDataAzureScheduler_Versions PRIMARY KEY (ID,Version)
  )
  print ' table DocDataAzureScheduler_Versions created' 
end

if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler_Versions') and (COLUMN_NAME = 'Name'))) begin  
     alter table DocDataAzureScheduler_Versions add    Name nvarchar(255) NULL
 print ' column Name added' 
end 
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler_Versions') and (COLUMN_NAME = 'Code'))) begin  
     alter table DocDataAzureScheduler_Versions add    Code nvarchar(255) NULL
 print ' column Code added' 
end 
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler_Versions') and (COLUMN_NAME = 'Enabled'))) begin  
     alter table DocDataAzureScheduler_Versions add    Enabled bit  NULL
 print ' column Enabled added' 
end 
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler_Versions') and (COLUMN_NAME = 'MbSchedulerID'))) begin  
     alter table DocDataAzureScheduler_Versions add    MbSchedulerID int NULL
 print ' column MbSchedulerID added' 
end 
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler_Versions') and (COLUMN_NAME = 'StartDate'))) begin  
     alter table DocDataAzureScheduler_Versions add    StartDate datetime  NULL
 print ' column StartDate added' 
end 
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler_Versions') and (COLUMN_NAME = 'EndDate'))) begin  
     alter table DocDataAzureScheduler_Versions add    EndDate datetime  NULL
 print ' column EndDate added' 
end 
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler_Versions') and (COLUMN_NAME = 'StartTime'))) begin  
     alter table DocDataAzureScheduler_Versions add    StartTime nvarchar(255) NULL
 print ' column StartTime added' 
end 
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler_Versions') and (COLUMN_NAME = 'EndTime'))) begin  
     alter table DocDataAzureScheduler_Versions add    EndTime nvarchar(255) NULL
 print ' column EndTime added' 
end 
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler_Versions') and (COLUMN_NAME = 'FrequencyType'))) begin  
     alter table DocDataAzureScheduler_Versions add    FrequencyType int NULL
 print ' column FrequencyType added' 
end 
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler_Versions') and (COLUMN_NAME = 'IntervalInSeconds'))) begin  
     alter table DocDataAzureScheduler_Versions add    IntervalInSeconds int NULL
 print ' column IntervalInSeconds added' 
end 
if not exists (select * from INFORMATION_SCHEMA.COLUMNS 
   where ((TABLE_NAME = 'DocDataAzureScheduler_Versions') and (COLUMN_NAME = 'Command'))) begin  
     alter table DocDataAzureScheduler_Versions add    Command nvarchar(max) NULL
 print ' column Command added' 
end 




exec sp_meta_GenerateCalcProcedure @meta_DocumentID
exec sp_GrantAll
-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------
---------------       DOCUMENT WORKFLOW SCHEME          ---------------------------------
-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------

-------------------------------------------------------- 
-- Canopus Macrobank Document Workflow Scheme SQL script 
-- Generated 2020-01-29 11:15:32
-- DocClass: AzureScheduler(Azure Scheduler)
-------------------------------------------------------- 
begin Transaction
Set nocount on
set XACT_ABORT on
-- Remove Old Data
declare @DcShortName nvarchar(255)
declare @DocClass int
set @DcShortName='AzureScheduler'
set @DocClass=0
if Exists(select * from DocClasses where ShortName=@DcShortName) 
begin
  select @DocClass=ID from DocClasses where ShortName=@DcShortName
  delete from DocFlowFieldLevelSecurity where FolderId in (select id from DocPathFolders where DocClass =@DocClass)
  delete from DocFlowFieldLevelSecurity where MethodId in (select id from DocPathMethods where DocClass =@DocClass)
  delete from DocPathMethods  where DocClass  =@DocClass and MethodGUID not in ('{067135C8-4AD4-477D-A6DD-0FAA5D8B7A81}','{8CEA5D1B-BEAA-4141-9A5B-548ED36DC86C}','{956599EC-A678-48BC-A4E7-FC8A2DD6326E}','{11F190F8-35F0-4F8A-AD06-F4956398112C}','{79DBCB0B-D774-4B55-9B86-888338201212}')
  delete from DocSubClasses   where DocClassID=@DocClass
end
  else
begin 
  insert into DocClasses(Name,ShortName, DocTableName, DocViewName, AccountField, Account2Field, Tag)     values (N'Azure Scheduler',N'AzureScheduler', N'DocDataAzureScheduler', N'', N'', N'', 0 )
  select @DocClass=@@IDENTITY
end 
-- SubClass data
--Variables
delete from SystemSettings where FolderID=-1 and Name like '*'+@DCShortName+'*%'
-- Model
  insert into DocPathModels_Versions (DocClass, Data, UpdatedUser, UpdatedAt, UpdatedTerminal, UpdatedLog) select DocClass, Data, UpdatedUser, UpdatedAt, UpdatedTerminal, UpdatedLog from DocPathModels where DocClass=@DocClass
  delete from DocPathModels   where DocClass  =@DocClass
insert into DocPathModels (DocClass,Data) values (@DocClass,N'<DocFlowScheme SchemeVersion="8" FullXML="True">
  <Items ItemsCount="18" NID="20">
    <Item ClsID="8" Name="" ItemID="1" Selected="False" Sizeable="False" Shaded="True" Caption="void" ImageIndex="0" ID="-1" UID="0" GUID="{23F52A2E-C7B9-4D68-849F-C7CD0E848AB0}" FontColor="0" FontStyle="0" PictureID="0" Scale="100" Mode="0" AllowModify="True" RequireUserAttention="False" SuspiciousActivity="False" StartingDocSubClassID="0" EventType="0" tp_days="0" tp_hours="0" tp_minutes="0" WorkingDaysOnly="False" TryCount="0" Condition="" OKMethodGUID="" CancelMethodGUID="">
      <PSD PageCount="1">
        <OPD PageID="0" Left="76" Top="264" Width="48" Height="48" Color="clBlack" BGColor="clWindow" PenWidth="1" FN="" FS="8" FBOL="True" FITA="False" FUND="False" FC="0" FCH="0" />
      </PSD>
      <Connections Count="1">
        <Connection ItemID="4" />
      </Connections>
      <Security FLSecurity2Operator="False" FLSecurity2Client="False" View2Operator="True" Edit2Operator="True" Delete2Operator="True" Restart2Operator="True" View2Client="True" Edit2Client="False" Delete2Client="False" Restart2Client="False" />
      <Tag />
    </Item>
    <Item ClsID="8" Name="" ItemID="2" Selected="False" Sizeable="False" Shaded="True" Caption="enabled" ImageIndex="0" ID="-2" UID="0" GUID="{BD38791A-1934-416F-A140-82F25165E1FA}" FontColor="0" FontStyle="0" PictureID="0" Scale="100" Mode="1" AllowModify="True" RequireUserAttention="False" SuspiciousActivity="False" StartingDocSubClassID="0" EventType="0" tp_days="0" tp_hours="0" tp_minutes="0" WorkingDaysOnly="False" TryCount="0" Condition="" OKMethodGUID="" CancelMethodGUID="">
      <PSD PageCount="1">
        <OPD PageID="0" Left="316" Top="100" Width="48" Height="48" Color="clBlack" BGColor="clWindow" PenWidth="1" FN="" FS="8" FBOL="True" FITA="False" FUND="False" FC="0" FCH="0" />
      </PSD>
      <Connections Count="4">
        <Connection ItemID="7" />
        <Connection ItemID="8" />
        <Connection ItemID="12" />
        <Connection ItemID="15" />
      </Connections>
      <Security FLSecurity2Operator="False" FLSecurity2Client="False" View2Operator="True" Edit2Operator="False" Delete2Operator="False" Restart2Operator="False" View2Client="True" Edit2Client="False" Delete2Client="False" Restart2Client="False" />
      <Tag />
    </Item>
    <Item ClsID="9" Name="" ItemID="3" Selected="True" Sizeable="False" Shaded="False" Caption="Create" ImageIndex="0" ID="-3" UID="0" GUID="{067135C8-4AD4-477D-A6DD-0FAA5D8B7A81}" FontColor="0" FontStyle="0" PictureID="0" Scale="100" MethodType="normal" E1E="True" E2E="False" E3E="False" NoAction="False" IsDefault="True" AllowRollback="False" HasNoRealAction="False" PushProcedureName="spdf_AzureScheduler_Create" RollbackProcedureName="" Active="True" CallDialogBeforeExecute="False" MethodHasNoProcedure="False" ApprovalRequired="False" Scheduled="False">
      <PSD PageCount="1">
        <OPD PageID="0" Left="196" Top="268" Width="42" Height="38" Color="clBlack" BGColor="clWindow" PenWidth="1" FN="" FS="8" FBOL="False" FITA="True" FUND="False" FC="0" FCH="0" />
      </PSD>
      <Connections Count="2">
        <Connection ItemID="4" />
        <Connection ItemID="16" />
      </Connections>
      <Security FLSecurity2Operator="False" FLSecurity2Client="False" Available2Operator="True" Available2Client="True" DialogBeforePush2Operator="False" DialogBeforePush2Client="False" ClientSignatureRequired="False" />
      <Tag />
      <Parameters />
      <PushMethod Version="6">
        <CSItem ClassName="TCaseStudioStartPoint" id="1" object_class="start_point" caption="START" tag="0" tag2="0" NextObjectID="2">
          <text />
          <variables />
          <position x1="10" x2="69" y1="30" y2="49" xc="40" yc="40" />
        </CSItem>
        <CSItem ClassName="TCaseStudioLoadData" id="2" object_class="load_data" caption="Load data" tag="0" tag2="0" NextObjectID="3" LoadDocumentsFields="" LoadSettings="">
          <text />
          <variables />
          <position x1="107" x2="142" y1="87" y2="122" xc="125" yc="105" />
        </CSItem>
        <CSItem ClassName="TCaseStudioProcess" id="3" object_class="process" caption="insert task" tag="0" tag2="0" NextObjectID="4">
          <text>
            declare @FrequencyTypeStr nvarchar(128) = case when @FrequencyType = 1 then ''EVERY-FEW-SECONDS'' when @FrequencyType = 2 then ''ONCE-A-DAY'' end&CRdeclare @UserNameTerm nvarchar(128) = SUSER_NAME()+'' ''+HOST_NAME()   &CR&CR&CRinsert into integrations.MbSchedulerTasks (MbSchedulerType,MbSchedulerCode,MbSchedulerName,MbSchedulerCommand,&CR  Enabled,                    &CR  ActiveStartDate_YYYYMMDD, ActiveEndDate_YYYYMMDD,  &CR  FrequencyType,&CR  ActiveStartTime_HHMMSS, ActiveEndTime_HHMMSS,  &CR  IntervalInSecondsBetweenRuns,        &CR  CreatedOn,CreatedBy,&CR  UpdatedOn, UpdatedBy &CR  )&CRvalues (''CALL-SQL-PROCEDURE'', @Code, @Name, @Command, &CR  0,  &CR  convert(varchar,@StartDate,112), convert(varchar,@EndDate,112),      &CR  @FrequencyTypeStr,&CR  @StartTime, @EndTime,     &CR  @IntervalInSeconds,  &CR  getdate(), @UserNameTerm,&CR  getdate(), @UserNameTerm&CR  )&CR&CRset @MbSchedulerID =  @@IDENTITY&CR&CRupdate DocDataAzureScheduler&CRset MbSchedulerID = @MbSchedulerID &CRwhere id = @DocumentID&CR
          </text>
          <variables />
          <position x1="80" x2="229" y1="196" y2="225" xc="155" yc="211" />
        </CSItem>
        <CSItem ClassName="TCaseStudioStopPoint" id="4" object_class="stop_point" caption="Exit 1" tag="0" tag2="1" ClientIDField="" TemplateShortName="">
          <text />
          <variables />
          <position x1="132" x2="191" y1="282" y2="301" xc="162" yc="292" />
          <TemplateFields />
        </CSItem>
      </PushMethod>
    </Item>
    <Item ClsID="10" Name="" ItemID="4" Selected="False" DrawPoints="False" LineStyle="0" FirstBox_Item_ID="1" SecondBox_Item_ID="3" Line_Direction="True" MethodEN="0">
      <PSD PageCount="1">
        <OPD PageID="0" PenWidth="1" Color="clBlack" FPD="0" LPD="0" CPos="5">
          <Points Count="4">
            <Point X="124" Y="288" />
            <Point X="165" Y="288" />
            <Point X="165" Y="287" />
            <Point X="196" Y="287" />
          </Points>
        </OPD>
      </PSD>
    </Item>
    <Item ClsID="9" Name="" ItemID="6" Selected="False" Sizeable="False" Shaded="False" Caption="Edit" ImageIndex="0" ID="-4" UID="0" GUID="{8CEA5D1B-BEAA-4141-9A5B-548ED36DC86C}" FontColor="0" FontStyle="0" PictureID="0" Scale="100" MethodType="normal" E1E="True" E2E="False" E3E="False" NoAction="False" IsDefault="False" AllowRollback="False" HasNoRealAction="False" PushProcedureName="spdf_AzureScheduler_Edit_Enabled" RollbackProcedureName="" Active="True" CallDialogBeforeExecute="False" MethodHasNoProcedure="False" ApprovalRequired="False" Scheduled="False">
      <PSD PageCount="1">
        <OPD PageID="0" Left="316" Top="24" Width="42" Height="38" Color="clBlack" BGColor="clWindow" PenWidth="1" FN="" FS="8" FBOL="False" FITA="True" FUND="False" FC="0" FCH="0" />
      </PSD>
      <Connections Count="2">
        <Connection ItemID="7" />
        <Connection ItemID="8" />
      </Connections>
      <Security FLSecurity2Operator="False" FLSecurity2Client="False" Available2Operator="True" Available2Client="True" DialogBeforePush2Operator="True" DialogBeforePush2Client="False" ClientSignatureRequired="False" />
      <Tag />
      <Parameters />
      <PushMethod Version="6">
        <CSItem ClassName="TCaseStudioStartPoint" id="1" object_class="start_point" caption="START" tag="0" tag2="0" NextObjectID="2">
          <text />
          <variables />
          <position x1="10" x2="69" y1="30" y2="49" xc="40" yc="40" />
        </CSItem>
        <CSItem ClassName="TCaseStudioProcess" id="2" object_class="process" caption="call Edit_Disabled" tag="0" tag2="0" NextObjectID="3">
          <text>
            exec @RC = spdf_AzureScheduler_Edit_Disabled @DocumentID, @EventID&CRreturn @RC&CR
          </text>
          <variables />
          <position x1="67" x2="216" y1="93" y2="122" xc="142" yc="108" />
        </CSItem>
        <CSItem ClassName="TCaseStudioStopPoint" id="3" object_class="stop_point" caption="Exit 1" tag="0" tag2="1" ClientIDField="" TemplateShortName="">
          <text />
          <variables />
          <position x1="153" x2="212" y1="155" y2="174" xc="183" yc="165" />
          <TemplateFields />
        </CSItem>
      </PushMethod>
    </Item>
    <Item ClsID="10" Name="" ItemID="7" Selected="False" DrawPoints="False" LineStyle="0" FirstBox_Item_ID="2" SecondBox_Item_ID="6" Line_Direction="True" MethodEN="1">
      <PSD PageCount="1">
        <OPD PageID="0" PenWidth="1" Color="clBlack" FPD="0" LPD="0" CPos="-5">
          <Points Count="4">
            <Point X="332" Y="100" />
            <Point X="332" Y="76" />
            <Point X="330" Y="76" />
            <Point X="330" Y="62" />
          </Points>
        </OPD>
      </PSD>
    </Item>
    <Item ClsID="10" Name="" ItemID="8" Selected="False" DrawPoints="False" LineStyle="0" FirstBox_Item_ID="6" SecondBox_Item_ID="2" Line_Direction="True" MethodEN="1">
      <PSD PageCount="1">
        <OPD PageID="0" PenWidth="1" Color="clBlack" FPD="0" LPD="0" CPos="5">
          <Points Count="4">
            <Point X="344" Y="62" />
            <Point X="344" Y="86" />
            <Point X="348" Y="86" />
            <Point X="348" Y="100" />
          </Points>
        </OPD>
      </PSD>
    </Item>
    <Item ClsID="8" Name="" ItemID="9" Selected="False" Sizeable="False" Shaded="True" Caption="disabled" ImageIndex="0" ID="-5" UID="0" GUID="{ED3A977E-856E-4B29-8B58-7EC7618A587A}" FontColor="0" FontStyle="0" PictureID="0" Scale="100" Mode="1" AllowModify="True" RequireUserAttention="False" SuspiciousActivity="False" StartingDocSubClassID="0" EventType="0" tp_days="0" tp_hours="0" tp_minutes="0" WorkingDaysOnly="False" TryCount="0" Condition="" OKMethodGUID="" CancelMethodGUID="">
      <PSD PageCount="1">
        <OPD PageID="0" Left="320" Top="264" Width="48" Height="48" Color="clBlack" BGColor="clWindow" PenWidth="1" FN="" FS="8" FBOL="True" FITA="False" FUND="False" FC="0" FCH="0" />
      </PSD>
      <Connections Count="5">
        <Connection ItemID="13" />
        <Connection ItemID="14" />
        <Connection ItemID="16" />
        <Connection ItemID="18" />
        <Connection ItemID="19" />
      </Connections>
      <Security FLSecurity2Operator="False" FLSecurity2Client="False" View2Operator="True" Edit2Operator="False" Delete2Operator="False" Restart2Operator="False" View2Client="True" Edit2Client="False" Delete2Client="False" Restart2Client="False" />
      <Tag />
    </Item>
    <Item ClsID="9" Name="" ItemID="10" Selected="False" Sizeable="False" Shaded="False" Caption="Disable" ImageIndex="0" ID="-6" UID="0" GUID="{956599EC-A678-48BC-A4E7-FC8A2DD6326E}" FontColor="0" FontStyle="0" PictureID="0" Scale="100" MethodType="normal" E1E="True" E2E="False" E3E="False" NoAction="False" IsDefault="False" AllowRollback="False" HasNoRealAction="False" PushProcedureName="spdf_AzureScheduler_Disable" RollbackProcedureName="" Active="True" CallDialogBeforeExecute="False" MethodHasNoProcedure="False" ApprovalRequired="False" Scheduled="False">
      <PSD PageCount="1">
        <OPD PageID="0" Left="284" Top="200" Width="42" Height="38" Color="clSilver" BGColor="clWindow" PenWidth="1" FN="" FS="8" FBOL="False" FITA="True" FUND="False" FC="0" FCH="0" />
      </PSD>
      <Connections Count="2">
        <Connection ItemID="12" />
        <Connection ItemID="13" />
      </Connections>
      <Security FLSecurity2Operator="False" FLSecurity2Client="False" Available2Operator="True" Available2Client="True" DialogBeforePush2Operator="False" DialogBeforePush2Client="False" ClientSignatureRequired="False" />
      <Tag />
      <Parameters />
      <PushMethod Version="6">
        <CSItem ClassName="TCaseStudioStartPoint" id="1" object_class="start_point" caption="START" tag="0" tag2="0" NextObjectID="2">
          <text />
          <variables />
          <position x1="10" x2="69" y1="30" y2="49" xc="40" yc="40" />
        </CSItem>
        <CSItem ClassName="TCaseStudioLoadData" id="2" object_class="load_data" caption="Load data" tag="0" tag2="0" NextObjectID="3" LoadDocumentsFields="" LoadSettings="">
          <text />
          <variables />
          <position x1="117" x2="152" y1="120" y2="155" xc="135" yc="138" />
        </CSItem>
        <CSItem ClassName="TCaseStudioProcess" id="3" object_class="process" caption="update task" tag="0" tag2="0" NextObjectID="4">
          <text>
            declare @UserNameTerm nvarchar(128) = SUSER_NAME()+'' ''+HOST_NAME()   &CR&CR&CRupdate integrations.MbSchedulerTasks set &CR  Enabled = 0,&CR  UpdatedOn = getdate(), UpdatedBy = @UserNameTerm &CRwhere MbSchedulerID = @MbSchedulerID&CR
          </text>
          <variables />
          <position x1="92" x2="241" y1="228" y2="257" xc="167" yc="243" />
        </CSItem>
        <CSItem ClassName="TCaseStudioStopPoint" id="4" object_class="stop_point" caption="Exit 1" tag="0" tag2="1" ClientIDField="" TemplateShortName="">
          <text />
          <variables />
          <position x1="144" x2="203" y1="314" y2="333" xc="174" yc="324" />
          <TemplateFields />
        </CSItem>
      </PushMethod>
    </Item>
    <Item ClsID="9" Name="" ItemID="11" Selected="False" Sizeable="False" Shaded="False" Caption="Enable" ImageIndex="0" ID="-7" UID="0" GUID="{11F190F8-35F0-4F8A-AD06-F4956398112C}" FontColor="0" FontStyle="0" PictureID="0" Scale="100" MethodType="normal" E1E="True" E2E="False" E3E="False" NoAction="False" IsDefault="False" AllowRollback="False" HasNoRealAction="False" PushProcedureName="spdf_AzureScheduler_Enable" RollbackProcedureName="" Active="True" CallDialogBeforeExecute="False" MethodHasNoProcedure="False" ApprovalRequired="False" Scheduled="False">
      <PSD PageCount="1">
        <OPD PageID="0" Left="352" Top="200" Width="42" Height="38" Color="clBlack" BGColor="clWindow" PenWidth="1" FN="" FS="8" FBOL="False" FITA="True" FUND="False" FC="0" FCH="0" />
      </PSD>
      <Connections Count="2">
        <Connection ItemID="14" />
        <Connection ItemID="15" />
      </Connections>
      <Security FLSecurity2Operator="False" FLSecurity2Client="False" Available2Operator="True" Available2Client="True" DialogBeforePush2Operator="False" DialogBeforePush2Client="False" ClientSignatureRequired="False" />
      <Tag />
      <Parameters />
      <PushMethod Version="6">
        <CSItem ClassName="TCaseStudioStartPoint" id="1" object_class="start_point" caption="START" tag="0" tag2="0" NextObjectID="2">
          <text />
          <variables />
          <position x1="10" x2="69" y1="30" y2="49" xc="40" yc="40" />
        </CSItem>
        <CSItem ClassName="TCaseStudioLoadData" id="2" object_class="load_data" caption="Load data" tag="0" tag2="0" NextObjectID="3" LoadDocumentsFields="" LoadSettings="">
          <text />
          <variables />
          <position x1="205" x2="240" y1="188" y2="223" xc="223" yc="206" />
        </CSItem>
        <CSItem ClassName="TCaseStudioProcess" id="3" object_class="process" caption="update task" tag="0" tag2="0" NextObjectID="4">
          <text>
            declare @UserNameTerm nvarchar(128) = SUSER_NAME()+'' ''+HOST_NAME()   &CR&CR&CRupdate integrations.MbSchedulerTasks set &CR  Enabled = 1,&CR  UpdatedOn = getdate(), UpdatedBy = @UserNameTerm &CRwhere MbSchedulerID = @MbSchedulerID&CR
          </text>
          <variables />
          <position x1="180" x2="329" y1="296" y2="325" xc="255" yc="311" />
        </CSItem>
        <CSItem ClassName="TCaseStudioStopPoint" id="4" object_class="stop_point" caption="Exit 1" tag="0" tag2="1" ClientIDField="" TemplateShortName="">
          <text />
          <variables />
          <position x1="232" x2="291" y1="382" y2="401" xc="262" yc="392" />
          <TemplateFields />
        </CSItem>
      </PushMethod>
    </Item>
    <Item ClsID="10" Name="" ItemID="12" Selected="False" DrawPoints="False" LineStyle="0" FirstBox_Item_ID="2" SecondBox_Item_ID="10" Line_Direction="True" MethodEN="1">
      <PSD PageCount="1">
        <OPD PageID="0" PenWidth="1" Color="clBlack" FPD="0" LPD="0" CPos="5">
          <Points Count="4">
            <Point X="332" Y="148" />
            <Point X="332" Y="179" />
            <Point X="305" Y="179" />
            <Point X="305" Y="200" />
          </Points>
        </OPD>
      </PSD>
    </Item>
    <Item ClsID="10" Name="" ItemID="13" Selected="False" DrawPoints="False" LineStyle="0" FirstBox_Item_ID="10" SecondBox_Item_ID="9" Line_Direction="True" MethodEN="1">
      <PSD PageCount="1">
        <OPD PageID="0" PenWidth="1" Color="clBlack" FPD="0" LPD="0" CPos="5">
          <Points Count="4">
            <Point X="305" Y="238" />
            <Point X="305" Y="256" />
            <Point X="336" Y="256" />
            <Point X="336" Y="264" />
          </Points>
        </OPD>
      </PSD>
    </Item>
    <Item ClsID="10" Name="" ItemID="14" Selected="False" DrawPoints="False" LineStyle="0" FirstBox_Item_ID="9" SecondBox_Item_ID="11" Line_Direction="True" MethodEN="1">
      <PSD PageCount="1">
        <OPD PageID="0" PenWidth="1" Color="clBlack" FPD="0" LPD="0" CPos="-5">
          <Points Count="4">
            <Point X="352" Y="264" />
            <Point X="352" Y="246" />
            <Point X="373" Y="246" />
            <Point X="373" Y="238" />
          </Points>
        </OPD>
      </PSD>
    </Item>
    <Item ClsID="10" Name="" ItemID="15" Selected="False" DrawPoints="False" LineStyle="0" FirstBox_Item_ID="11" SecondBox_Item_ID="2" Line_Direction="True" MethodEN="1">
      <PSD PageCount="1">
        <OPD PageID="0" PenWidth="1" Color="clBlack" FPD="0" LPD="0" CPos="-5">
          <Points Count="4">
            <Point X="373" Y="200" />
            <Point X="373" Y="169" />
            <Point X="348" Y="169" />
            <Point X="348" Y="148" />
          </Points>
        </OPD>
      </PSD>
    </Item>
    <Item ClsID="10" Name="" ItemID="16" Selected="False" DrawPoints="False" LineStyle="0" FirstBox_Item_ID="3" SecondBox_Item_ID="9" Line_Direction="True" MethodEN="1">
      <PSD PageCount="1">
        <OPD PageID="0" PenWidth="1" Color="clBlack" FPD="0" LPD="0" CPos="5">
          <Points Count="4">
            <Point X="238" Y="287" />
            <Point X="284" Y="287" />
            <Point X="284" Y="288" />
            <Point X="320" Y="288" />
          </Points>
        </OPD>
      </PSD>
    </Item>
    <Item ClsID="9" Name="" ItemID="17" Selected="False" Sizeable="False" Shaded="False" Caption="Edit" ImageIndex="0" ID="-8" UID="0" GUID="{79DBCB0B-D774-4B55-9B86-888338201212}" FontColor="0" FontStyle="0" PictureID="0" Scale="100" MethodType="normal" E1E="True" E2E="False" E3E="False" NoAction="False" IsDefault="False" AllowRollback="False" HasNoRealAction="False" PushProcedureName="spdf_AzureScheduler_Edit_Disabled" RollbackProcedureName="" Active="True" CallDialogBeforeExecute="False" MethodHasNoProcedure="False" ApprovalRequired="False" Scheduled="False">
      <PSD PageCount="1">
        <OPD PageID="0" Left="328" Top="372" Width="42" Height="38" Color="clSilver" BGColor="clWindow" PenWidth="1" FN="" FS="8" FBOL="False" FITA="True" FUND="False" FC="0" FCH="0" />
      </PSD>
      <Connections Count="2">
        <Connection ItemID="18" />
        <Connection ItemID="19" />
      </Connections>
      <Security FLSecurity2Operator="False" FLSecurity2Client="False" Available2Operator="True" Available2Client="True" DialogBeforePush2Operator="True" DialogBeforePush2Client="False" ClientSignatureRequired="False" />
      <Tag />
      <Parameters />
      <PushMethod Version="6">
        <CSItem ClassName="TCaseStudioStartPoint" id="1" object_class="start_point" caption="START" tag="0" tag2="0" NextObjectID="2">
          <text />
          <variables />
          <position x1="10" x2="69" y1="30" y2="49" xc="40" yc="40" />
        </CSItem>
        <CSItem ClassName="TCaseStudioLoadData" id="2" object_class="load_data" caption="Load data" tag="0" tag2="0" NextObjectID="3" LoadDocumentsFields="" LoadSettings="">
          <text />
          <variables />
          <position x1="157" x2="192" y1="137" y2="172" xc="175" yc="155" />
        </CSItem>
        <CSItem ClassName="TCaseStudioProcess" id="3" object_class="process" caption="update task" tag="0" tag2="0" NextObjectID="4">
          <text>
            declare @FrequencyTypeStr nvarchar(128) = case when @FrequencyType = 1 then ''EVERY-FEW-SECONDS'' when @FrequencyType = 2 then ''ONCE-A-DAY'' end&CRdeclare @UserNameTerm nvarchar(128) = SUSER_NAME()+'' ''+HOST_NAME()   &CR&CR&CRupdate integrations.MbSchedulerTasks set &CR  MbSchedulerCode = @Code,&CR  MbSchedulerName = @Name,&CR  MbSchedulerCommand= @Command,&CR  ActiveStartDate_YYYYMMDD = convert(varchar,@StartDate,112),&CR  ActiveEndDate_YYYYMMDD = convert(varchar,@EndDate,112),  &CR  FrequencyType = @FrequencyTypeStr,&CR  ActiveStartTime_HHMMSS = @StartTime,&CR  ActiveEndTime_HHMMSS = @EndTime,  &CR  IntervalInSecondsBetweenRuns = @IntervalInSeconds,        &CR  UpdatedOn = getdate(), UpdatedBy = @UserNameTerm &CRwhere MbSchedulerID = @MbSchedulerID&CR
          </text>
          <variables />
          <position x1="130" x2="279" y1="246" y2="275" xc="205" yc="261" />
        </CSItem>
        <CSItem ClassName="TCaseStudioStopPoint" id="4" object_class="stop_point" caption="Exit 1" tag="0" tag2="1" ClientIDField="" TemplateShortName="">
          <text />
          <variables />
          <position x1="182" x2="241" y1="332" y2="351" xc="212" yc="342" />
          <TemplateFields />
        </CSItem>
      </PushMethod>
    </Item>
    <Item ClsID="10" Name="" ItemID="18" Selected="False" DrawPoints="False" LineStyle="0" FirstBox_Item_ID="9" SecondBox_Item_ID="17" Line_Direction="True" MethodEN="1">
      <PSD PageCount="1">
        <OPD PageID="0" PenWidth="1" Color="clBlack" FPD="0" LPD="0" CPos="5">
          <Points Count="4">
            <Point X="336" Y="312" />
            <Point X="336" Y="347" />
            <Point X="342" Y="347" />
            <Point X="342" Y="372" />
          </Points>
        </OPD>
      </PSD>
    </Item>
    <Item ClsID="10" Name="" ItemID="19" Selected="False" DrawPoints="False" LineStyle="0" FirstBox_Item_ID="17" SecondBox_Item_ID="9" Line_Direction="True" MethodEN="1">
      <PSD PageCount="1">
        <OPD PageID="0" PenWidth="1" Color="clBlack" FPD="0" LPD="0" CPos="-5">
          <Points Count="4">
            <Point X="356" Y="372" />
            <Point X="356" Y="337" />
            <Point X="352" Y="337" />
            <Point X="352" Y="312" />
          </Points>
        </OPD>
      </PSD>
    </Item>
  </Items>
  <ModelPages>
    <ModelPage Name="Untitled Page 1" PageID="0" PZoom="100" POrgX="0" POrgY="0" PageID="0">
      <PageObjects>
        <PI ItemID="1" />
        <PI ItemID="2" />
        <PI ItemID="3" />
        <PI ItemID="6" />
        <PI ItemID="9" />
        <PI ItemID="10" />
        <PI ItemID="11" />
        <PI ItemID="17" />
      </PageObjects>
    </ModelPage>
  </ModelPages>
  <ContainerOptions PageIndex="0" BGColor="16777215" />
  <SelectedItems Count="1">
    <SI ItemID="3" />
  </SelectedItems>
  <Font Name="Verdana" Size="8" />
  <PrinterOptions Orientation="0" FitToXPages="0" FitToYPages="0" />
  <SubClasses />
  <Variables />
</DocFlowScheme>
')

--Folders
if not Exists(select ID from DocPathFolders where FolderGUID=N'{23F52A2E-C7B9-4D68-849F-C7CD0E848AB0}')
  INSERT INTO DocPathFolders( 
    DocClass, Description, AllowModify, 
    Mode, FontColor, FontStyle,   
    PictureID, FolderUID, RequireUserAttention, 
    StartingDocSubClassID,  
    WaitingForEvent, EventType, TP_Days, TP_Hours, TP_Minutes, 
    WorkingDaysOnly, TryCount, Condition, OKMethodID, CancelMethodID, 
    SuspiciousActivity , FolderGUID ,    View2Operator, Delete2Operator, Edit2Operator, Restart2Operator, FLSecurity2Operator,    View2Client, Delete2Client, Edit2Client, Restart2Client, FLSecurity2Client, Tag     ) VALUES (
    @DocClass, N'void', 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, N'', 0, 0, 0, N'{23F52A2E-C7B9-4D68-849F-C7CD0E848AB0}',     1, 1, 1, 1, 0, 1, 0, 0, 0, 0, N'')
Else
  UPDATE DocPathFolders set 
    Description=N'void',     AllowModify=1, 
    Mode=0,     FontColor=0,     FontStyle=0, 
    PictureID=0,     FolderUID=0,     RequireUserAttention=0, 
    StartingDocSubClassID=0, 
    WaitingForEvent=1,     EventType=0,     TP_Days=0,     TP_Hours=0,     TP_Minutes=0, 
    WorkingDaysOnly=0,     TryCount=0,     Condition=N'',     OKMethodID=0,     CancelMethodID=0, 
    SuspiciousActivity=0,
    View2Operator=1,    Delete2Operator=1,    Edit2Operator=1,    Restart2Operator=1,    FLSecurity2Operator=0,
    View2Client=1,    Delete2Client=0,    Edit2Client=0,    Restart2Client=0,    FLSecurity2Client=0,
    Tag=N''
  where FolderGUID=N'{23F52A2E-C7B9-4D68-849F-C7CD0E848AB0}'

if not Exists(select ID from DocPathFolders where FolderGUID=N'{BD38791A-1934-416F-A140-82F25165E1FA}')
  INSERT INTO DocPathFolders( 
    DocClass, Description, AllowModify, 
    Mode, FontColor, FontStyle,   
    PictureID, FolderUID, RequireUserAttention, 
    StartingDocSubClassID,  
    WaitingForEvent, EventType, TP_Days, TP_Hours, TP_Minutes, 
    WorkingDaysOnly, TryCount, Condition, OKMethodID, CancelMethodID, 
    SuspiciousActivity , FolderGUID ,    View2Operator, Delete2Operator, Edit2Operator, Restart2Operator, FLSecurity2Operator,    View2Client, Delete2Client, Edit2Client, Restart2Client, FLSecurity2Client, Tag     ) VALUES (
    @DocClass, N'enabled', 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, N'', 0, 0, 0, N'{BD38791A-1934-416F-A140-82F25165E1FA}',     1, 0, 0, 0, 0, 1, 0, 0, 0, 0, N'')
Else
  UPDATE DocPathFolders set 
    Description=N'enabled',     AllowModify=1, 
    Mode=1,     FontColor=0,     FontStyle=0, 
    PictureID=0,     FolderUID=0,     RequireUserAttention=0, 
    StartingDocSubClassID=0, 
    WaitingForEvent=1,     EventType=0,     TP_Days=0,     TP_Hours=0,     TP_Minutes=0, 
    WorkingDaysOnly=0,     TryCount=0,     Condition=N'',     OKMethodID=0,     CancelMethodID=0, 
    SuspiciousActivity=0,
    View2Operator=1,    Delete2Operator=0,    Edit2Operator=0,    Restart2Operator=0,    FLSecurity2Operator=0,
    View2Client=1,    Delete2Client=0,    Edit2Client=0,    Restart2Client=0,    FLSecurity2Client=0,
    Tag=N''
  where FolderGUID=N'{BD38791A-1934-416F-A140-82F25165E1FA}'

if not Exists(select ID from DocPathFolders where FolderGUID=N'{ED3A977E-856E-4B29-8B58-7EC7618A587A}')
  INSERT INTO DocPathFolders( 
    DocClass, Description, AllowModify, 
    Mode, FontColor, FontStyle,   
    PictureID, FolderUID, RequireUserAttention, 
    StartingDocSubClassID,  
    WaitingForEvent, EventType, TP_Days, TP_Hours, TP_Minutes, 
    WorkingDaysOnly, TryCount, Condition, OKMethodID, CancelMethodID, 
    SuspiciousActivity , FolderGUID ,    View2Operator, Delete2Operator, Edit2Operator, Restart2Operator, FLSecurity2Operator,    View2Client, Delete2Client, Edit2Client, Restart2Client, FLSecurity2Client, Tag     ) VALUES (
    @DocClass, N'disabled', 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, N'', 0, 0, 0, N'{ED3A977E-856E-4B29-8B58-7EC7618A587A}',     1, 0, 0, 0, 0, 1, 0, 0, 0, 0, N'')
Else
  UPDATE DocPathFolders set 
    Description=N'disabled',     AllowModify=1, 
    Mode=1,     FontColor=0,     FontStyle=0, 
    PictureID=0,     FolderUID=0,     RequireUserAttention=0, 
    StartingDocSubClassID=0, 
    WaitingForEvent=1,     EventType=0,     TP_Days=0,     TP_Hours=0,     TP_Minutes=0, 
    WorkingDaysOnly=0,     TryCount=0,     Condition=N'',     OKMethodID=0,     CancelMethodID=0, 
    SuspiciousActivity=0,
    View2Operator=1,    Delete2Operator=0,    Edit2Operator=0,    Restart2Operator=0,    FLSecurity2Operator=0,
    View2Client=1,    Delete2Client=0,    Edit2Client=0,    Restart2Client=0,    FLSecurity2Client=0,
    Tag=N''
  where FolderGUID=N'{ED3A977E-856E-4B29-8B58-7EC7618A587A}'


create table #tmpMethodsLinks ( OldID int null,  NewID int null )
-- Methods
if not Exists(select ID from DocPathMethods where MethodGUID=N'{067135C8-4AD4-477D-A6DD-0FAA5D8B7A81}') 
BEGIN   INSERT INTO DocPathMethods(    DocClass, Description,    PushProcedureName, RollbackProcedureName, AllowDesktop,    AllowRollback, IsDefault, PictureID,    CallDialogBeforeExecute, MethodHasNoProcedure, ApprovalRequired, Scheduled,    UID, MethodGUID, SourceFolderGUID, TargetFolderGUID, TargetFolderGUID1 , TargetFolderGUID2,    Available2Operator, Available2Client, DialogBeforePush2Operator, DialogBeforePush2Client,    FLSecurity2Operator, FLSecurity2Client, ClientSignatureRequired,    MethodType, Parameters, Exits, Tag    )     Values (@DocClass,N'Create',N'spdf_AzureScheduler_Create',N'',1,0,1,0,0,0,0,0,0,      N'{067135C8-4AD4-477D-A6DD-0FAA5D8B7A81}',N'{23F52A2E-C7B9-4D68-849F-C7CD0E848AB0}',N'{ED3A977E-856E-4B29-8B58-7EC7618A587A}',N'', N'',      1, 1,0,0, 0,0,0, N'normal',N'',N'1',N'') 
  insert into #tmpMethodsLinks(OldID,NewID) values (2196,@@IDENTITY)
END Else BEGIN
  UPDATE DocPathMethods set 
    Description=N'Create', 
    PushProcedureName=N'spdf_AzureScheduler_Create',     RollbackProcedureName=N'',     AllowDesktop=1, 
    AllowRollback=0,     IsDefault=1,     PictureID=0, 
    CallDialogBeforeExecute=0,     MethodHasNoProcedure=0,     ApprovalRequired=0,     Scheduled=0, 
    UID=0,     SourceFolderGUID=N'{23F52A2E-C7B9-4D68-849F-C7CD0E848AB0}',     TargetFolderGUID=N'{ED3A977E-856E-4B29-8B58-7EC7618A587A}',     TargetFolderGUID1=N'',     TargetFolderGUID2=N'', 
    Available2Operator=1,     Available2Client=1,     DialogBeforePush2Operator=0,     DialogBeforePush2Client=0, 
    FLSecurity2Operator=0,    FLSecurity2Client=0,    ClientSignatureRequired=0,
    MethodType=N'normal',     Parameters=N'',     Exits=N'1',     Tag=N'' 
  where MethodGUID=N'{067135C8-4AD4-477D-A6DD-0FAA5D8B7A81}'
  delete from DocMethods_BlackListCheckers where MethodId=(select id from DocPathMethods where MethodGUID=N'{067135C8-4AD4-477D-A6DD-0FAA5D8B7A81}')
END

if not Exists(select ID from DocPathMethods where MethodGUID=N'{8CEA5D1B-BEAA-4141-9A5B-548ED36DC86C}') 
BEGIN   INSERT INTO DocPathMethods(    DocClass, Description,    PushProcedureName, RollbackProcedureName, AllowDesktop,    AllowRollback, IsDefault, PictureID,    CallDialogBeforeExecute, MethodHasNoProcedure, ApprovalRequired, Scheduled,    UID, MethodGUID, SourceFolderGUID, TargetFolderGUID, TargetFolderGUID1 , TargetFolderGUID2,    Available2Operator, Available2Client, DialogBeforePush2Operator, DialogBeforePush2Client,    FLSecurity2Operator, FLSecurity2Client, ClientSignatureRequired,    MethodType, Parameters, Exits, Tag    )     Values (@DocClass,N'Edit',N'spdf_AzureScheduler_Edit_Enabled',N'',1,0,0,0,0,0,0,0,0,      N'{8CEA5D1B-BEAA-4141-9A5B-548ED36DC86C}',N'{BD38791A-1934-416F-A140-82F25165E1FA}',N'{BD38791A-1934-416F-A140-82F25165E1FA}',N'', N'',      1, 1,1,0, 0,0,0, N'normal',N'',N'1',N'') 
  insert into #tmpMethodsLinks(OldID,NewID) values (2197,@@IDENTITY)
END Else BEGIN
  UPDATE DocPathMethods set 
    Description=N'Edit', 
    PushProcedureName=N'spdf_AzureScheduler_Edit_Enabled',     RollbackProcedureName=N'',     AllowDesktop=1, 
    AllowRollback=0,     IsDefault=0,     PictureID=0, 
    CallDialogBeforeExecute=0,     MethodHasNoProcedure=0,     ApprovalRequired=0,     Scheduled=0, 
    UID=0,     SourceFolderGUID=N'{BD38791A-1934-416F-A140-82F25165E1FA}',     TargetFolderGUID=N'{BD38791A-1934-416F-A140-82F25165E1FA}',     TargetFolderGUID1=N'',     TargetFolderGUID2=N'', 
    Available2Operator=1,     Available2Client=1,     DialogBeforePush2Operator=1,     DialogBeforePush2Client=0, 
    FLSecurity2Operator=0,    FLSecurity2Client=0,    ClientSignatureRequired=0,
    MethodType=N'normal',     Parameters=N'',     Exits=N'1',     Tag=N'' 
  where MethodGUID=N'{8CEA5D1B-BEAA-4141-9A5B-548ED36DC86C}'
  delete from DocMethods_BlackListCheckers where MethodId=(select id from DocPathMethods where MethodGUID=N'{8CEA5D1B-BEAA-4141-9A5B-548ED36DC86C}')
END

if not Exists(select ID from DocPathMethods where MethodGUID=N'{956599EC-A678-48BC-A4E7-FC8A2DD6326E}') 
BEGIN   INSERT INTO DocPathMethods(    DocClass, Description,    PushProcedureName, RollbackProcedureName, AllowDesktop,    AllowRollback, IsDefault, PictureID,    CallDialogBeforeExecute, MethodHasNoProcedure, ApprovalRequired, Scheduled,    UID, MethodGUID, SourceFolderGUID, TargetFolderGUID, TargetFolderGUID1 , TargetFolderGUID2,    Available2Operator, Available2Client, DialogBeforePush2Operator, DialogBeforePush2Client,    FLSecurity2Operator, FLSecurity2Client, ClientSignatureRequired,    MethodType, Parameters, Exits, Tag    )     Values (@DocClass,N'Disable',N'spdf_AzureScheduler_Disable',N'',1,0,0,0,0,0,0,0,0,      N'{956599EC-A678-48BC-A4E7-FC8A2DD6326E}',N'{BD38791A-1934-416F-A140-82F25165E1FA}',N'{ED3A977E-856E-4B29-8B58-7EC7618A587A}',N'', N'',      1, 1,0,0, 0,0,0, N'normal',N'',N'1',N'') 
  insert into #tmpMethodsLinks(OldID,NewID) values (2198,@@IDENTITY)
END Else BEGIN
  UPDATE DocPathMethods set 
    Description=N'Disable', 
    PushProcedureName=N'spdf_AzureScheduler_Disable',     RollbackProcedureName=N'',     AllowDesktop=1, 
    AllowRollback=0,     IsDefault=0,     PictureID=0, 
    CallDialogBeforeExecute=0,     MethodHasNoProcedure=0,     ApprovalRequired=0,     Scheduled=0, 
    UID=0,     SourceFolderGUID=N'{BD38791A-1934-416F-A140-82F25165E1FA}',     TargetFolderGUID=N'{ED3A977E-856E-4B29-8B58-7EC7618A587A}',     TargetFolderGUID1=N'',     TargetFolderGUID2=N'', 
    Available2Operator=1,     Available2Client=1,     DialogBeforePush2Operator=0,     DialogBeforePush2Client=0, 
    FLSecurity2Operator=0,    FLSecurity2Client=0,    ClientSignatureRequired=0,
    MethodType=N'normal',     Parameters=N'',     Exits=N'1',     Tag=N'' 
  where MethodGUID=N'{956599EC-A678-48BC-A4E7-FC8A2DD6326E}'
  delete from DocMethods_BlackListCheckers where MethodId=(select id from DocPathMethods where MethodGUID=N'{956599EC-A678-48BC-A4E7-FC8A2DD6326E}')
END

if not Exists(select ID from DocPathMethods where MethodGUID=N'{11F190F8-35F0-4F8A-AD06-F4956398112C}') 
BEGIN   INSERT INTO DocPathMethods(    DocClass, Description,    PushProcedureName, RollbackProcedureName, AllowDesktop,    AllowRollback, IsDefault, PictureID,    CallDialogBeforeExecute, MethodHasNoProcedure, ApprovalRequired, Scheduled,    UID, MethodGUID, SourceFolderGUID, TargetFolderGUID, TargetFolderGUID1 , TargetFolderGUID2,    Available2Operator, Available2Client, DialogBeforePush2Operator, DialogBeforePush2Client,    FLSecurity2Operator, FLSecurity2Client, ClientSignatureRequired,    MethodType, Parameters, Exits, Tag    )     Values (@DocClass,N'Enable',N'spdf_AzureScheduler_Enable',N'',1,0,0,0,0,0,0,0,0,      N'{11F190F8-35F0-4F8A-AD06-F4956398112C}',N'{ED3A977E-856E-4B29-8B58-7EC7618A587A}',N'{BD38791A-1934-416F-A140-82F25165E1FA}',N'', N'',      1, 1,0,0, 0,0,0, N'normal',N'',N'1',N'') 
  insert into #tmpMethodsLinks(OldID,NewID) values (2199,@@IDENTITY)
END Else BEGIN
  UPDATE DocPathMethods set 
    Description=N'Enable', 
    PushProcedureName=N'spdf_AzureScheduler_Enable',     RollbackProcedureName=N'',     AllowDesktop=1, 
    AllowRollback=0,     IsDefault=0,     PictureID=0, 
    CallDialogBeforeExecute=0,     MethodHasNoProcedure=0,     ApprovalRequired=0,     Scheduled=0, 
    UID=0,     SourceFolderGUID=N'{ED3A977E-856E-4B29-8B58-7EC7618A587A}',     TargetFolderGUID=N'{BD38791A-1934-416F-A140-82F25165E1FA}',     TargetFolderGUID1=N'',     TargetFolderGUID2=N'', 
    Available2Operator=1,     Available2Client=1,     DialogBeforePush2Operator=0,     DialogBeforePush2Client=0, 
    FLSecurity2Operator=0,    FLSecurity2Client=0,    ClientSignatureRequired=0,
    MethodType=N'normal',     Parameters=N'',     Exits=N'1',     Tag=N'' 
  where MethodGUID=N'{11F190F8-35F0-4F8A-AD06-F4956398112C}'
  delete from DocMethods_BlackListCheckers where MethodId=(select id from DocPathMethods where MethodGUID=N'{11F190F8-35F0-4F8A-AD06-F4956398112C}')
END

if not Exists(select ID from DocPathMethods where MethodGUID=N'{79DBCB0B-D774-4B55-9B86-888338201212}') 
BEGIN   INSERT INTO DocPathMethods(    DocClass, Description,    PushProcedureName, RollbackProcedureName, AllowDesktop,    AllowRollback, IsDefault, PictureID,    CallDialogBeforeExecute, MethodHasNoProcedure, ApprovalRequired, Scheduled,    UID, MethodGUID, SourceFolderGUID, TargetFolderGUID, TargetFolderGUID1 , TargetFolderGUID2,    Available2Operator, Available2Client, DialogBeforePush2Operator, DialogBeforePush2Client,    FLSecurity2Operator, FLSecurity2Client, ClientSignatureRequired,    MethodType, Parameters, Exits, Tag    )     Values (@DocClass,N'Edit',N'spdf_AzureScheduler_Edit_Disabled',N'',1,0,0,0,0,0,0,0,0,      N'{79DBCB0B-D774-4B55-9B86-888338201212}',N'{ED3A977E-856E-4B29-8B58-7EC7618A587A}',N'{ED3A977E-856E-4B29-8B58-7EC7618A587A}',N'', N'',      1, 1,1,0, 0,0,0, N'normal',N'',N'1',N'') 
  insert into #tmpMethodsLinks(OldID,NewID) values (2200,@@IDENTITY)
END Else BEGIN
  UPDATE DocPathMethods set 
    Description=N'Edit', 
    PushProcedureName=N'spdf_AzureScheduler_Edit_Disabled',     RollbackProcedureName=N'',     AllowDesktop=1, 
    AllowRollback=0,     IsDefault=0,     PictureID=0, 
    CallDialogBeforeExecute=0,     MethodHasNoProcedure=0,     ApprovalRequired=0,     Scheduled=0, 
    UID=0,     SourceFolderGUID=N'{ED3A977E-856E-4B29-8B58-7EC7618A587A}',     TargetFolderGUID=N'{ED3A977E-856E-4B29-8B58-7EC7618A587A}',     TargetFolderGUID1=N'',     TargetFolderGUID2=N'', 
    Available2Operator=1,     Available2Client=1,     DialogBeforePush2Operator=1,     DialogBeforePush2Client=0, 
    FLSecurity2Operator=0,    FLSecurity2Client=0,    ClientSignatureRequired=0,
    MethodType=N'normal',     Parameters=N'',     Exits=N'1',     Tag=N'' 
  where MethodGUID=N'{79DBCB0B-D774-4B55-9B86-888338201212}'
  delete from DocMethods_BlackListCheckers where MethodId=(select id from DocPathMethods where MethodGUID=N'{79DBCB0B-D774-4B55-9B86-888338201212}')
END


-- Reassign Folders
update DocPathMethods set 
  SourceFolderID=(select ID from DocPathFolders where FolderGUID=SourceFolderGUID),
  TargetFolderID=(select ID from DocPathFolders where FolderGUID=TargetFolderGUID),
  TargetFolderID1=(select ID from DocPathFolders where FolderGUID=TargetFolderGUID1),
  TargetFolderID2=(select ID from DocPathFolders where FolderGUID=TargetFolderGUID2)
 where 
   DocClass=@DocClass

-- Reassign Ok & Cancel Methods
update DocPathFolders set 
  OkMethodID=isnull( (select T.NewID from #tmpMethodsLinks T where T.OldID=-OkMethodID) , abs(OkMethodID) )
where 
  DocClass=@DocClass and OkMethodID < 0

update DocPathFolders set 
  CancelMethodID=isnull( (select T.NewID from #tmpMethodsLinks T where T.OldID=-CancelMethodID) , abs(CancelMethodID) )
where 
  DocClass=@DocClass and CancelMethodID < 0

drop table #tmpMethodsLinks

------------- Field Level Security-------------------------
------------  Finish FL Security  -------------------------

-- Tariff Groups
-- Tariffs
declare @TariffGroupID int
declare @TariffAccID int, @TariffCOAID int
declare @COATariffID int
declare @LocAccID int
declare @LocMethodID int, @LocTariffID int
-------------------------------------------------------- 
-- Commit changes
if XACT_STATE()=1 commit transaction else rollback transaction
Go


-------------------------------------------------------- 
-- Metadata block
-------------------------------------------------------- 
if exists (select * from dbo.sysobjects where id = object_id('spdf_AzureScheduler_Create') and OBJECTPROPERTY(id, 'IsProcedure') = 1)drop procedure spdf_AzureScheduler_Create
Go
CREATE PROCEDURE spdf_AzureScheduler_Create
  @DocumentID integer,
  @EventID    integer
AS
BEGIN

  --common
  DECLARE  @RC int
  --TCaseStudioLoadData
  DECLARE  @BlackListed_Accepted bit
  DECLARE  @ClientID integer
  DECLARE  @Code nvarchar(max)
  DECLARE  @Command nvarchar(max)
  DECLARE  @Enabled bit
  DECLARE  @EndDate datetime
  DECLARE  @EndTime nvarchar(max)
  DECLARE  @FrequencyType int
  DECLARE  @IntervalInSeconds int
  DECLARE  @MbSchedulerID int
  DECLARE  @Name nvarchar(max)
  DECLARE  @StartDate datetime
  DECLARE  @StartTime nvarchar(max)
  DECLARE  @TheDate datetime
  DECLARE  @#AmountIncludesTariff int
  DECLARE  @#ManualTariffAmount int
  --for block Load data
  DECLARE  @CurrentMethodID int

  
  
   /* BLOCK Load data */
  
  
  SET  @#ManualTariffAmount=NULL
  SET  @#AmountIncludesTariff=NULL
  
  SELECT @BlackListed_Accepted=BlackListed_Accepted, 
       @ClientID=ClientID, 
       @Code=Code, 
       @Command=Command, 
       @Enabled=Enabled, 
       @EndDate=EndDate, 
       @EndTime=EndTime, 
       @FrequencyType=FrequencyType, 
       @IntervalInSeconds=IntervalInSeconds, 
       @MbSchedulerID=MbSchedulerID, 
       @Name=Name, 
       @StartDate=StartDate, 
       @StartTime=StartTime, 
       @TheDate=TheDate
  FROM Documents left join DocDataAzureScheduler
    on Documents.ID=DocDataAzureScheduler.ID
  WHERE Documents.ID=@DocumentID
  
  select @CurrentMethodID=MethodID from DocEvents where ID=@EventID
  
  
  
   /* BLOCK insert task */
  
  
  declare @FrequencyTypeStr nvarchar(128) = case when @FrequencyType = 1 then 'EVERY-FEW-SECONDS' when @FrequencyType = 2 then 'ONCE-A-DAY' end
  declare @UserNameTerm nvarchar(128) = SUSER_NAME()+' '+HOST_NAME()   
  
  
  insert into integrations.MbSchedulerTasks (MbSchedulerType,MbSchedulerCode,MbSchedulerName,MbSchedulerCommand,
    Enabled,                    
    ActiveStartDate_YYYYMMDD, ActiveEndDate_YYYYMMDD,  
    FrequencyType,
    ActiveStartTime_HHMMSS, ActiveEndTime_HHMMSS,  
    IntervalInSecondsBetweenRuns,        
    CreatedOn,CreatedBy,
    UpdatedOn, UpdatedBy 
    )
  values ('CALL-SQL-PROCEDURE', @Code, @Name, @Command, 
    0,  
    convert(varchar,@StartDate,112), convert(varchar,@EndDate,112),      
    @FrequencyTypeStr,
    @StartTime, @EndTime,     
    @IntervalInSeconds,  
    getdate(), @UserNameTerm,
    getdate(), @UserNameTerm
    )
  
  set @MbSchedulerID =  @@IDENTITY
  
  update DocDataAzureScheduler
  set MbSchedulerID = @MbSchedulerID 
  where id = @DocumentID
  IF @@ERROR<>0 RETURN 0
  
  
   /* BLOCK Exit 1 */
  
  
  RETURN 1

END
Go

if exists (select * from dbo.sysobjects where id = object_id('spdf_AzureScheduler_Edit_Enabled') and OBJECTPROPERTY(id, 'IsProcedure') = 1)drop procedure spdf_AzureScheduler_Edit_Enabled
Go
CREATE PROCEDURE spdf_AzureScheduler_Edit_Enabled
  @DocumentID integer,
  @EventID    integer
AS
BEGIN

  --common
  DECLARE  @RC int

  
  
   /* BLOCK call Edit_Disabled */
  
  
  exec @RC = spdf_AzureScheduler_Edit_Disabled @DocumentID, @EventID
  return @RC
  IF @@ERROR<>0 RETURN 0
  
  
   /* BLOCK Exit 1 */
  
  
  RETURN 1

END
Go

if exists (select * from dbo.sysobjects where id = object_id('spdf_AzureScheduler_Disable') and OBJECTPROPERTY(id, 'IsProcedure') = 1)drop procedure spdf_AzureScheduler_Disable
Go
CREATE PROCEDURE spdf_AzureScheduler_Disable
  @DocumentID integer,
  @EventID    integer
AS
BEGIN

  --common
  DECLARE  @RC int
  --TCaseStudioLoadData
  DECLARE  @BlackListed_Accepted bit
  DECLARE  @ClientID integer
  DECLARE  @Code nvarchar(max)
  DECLARE  @Command nvarchar(max)
  DECLARE  @Enabled bit
  DECLARE  @EndDate datetime
  DECLARE  @EndTime nvarchar(max)
  DECLARE  @FrequencyType int
  DECLARE  @IntervalInSeconds int
  DECLARE  @MbSchedulerID int
  DECLARE  @Name nvarchar(max)
  DECLARE  @StartDate datetime
  DECLARE  @StartTime nvarchar(max)
  DECLARE  @TheDate datetime
  DECLARE  @#AmountIncludesTariff int
  DECLARE  @#ManualTariffAmount int
  --for block Load data
  DECLARE  @CurrentMethodID int

  
  
   /* BLOCK Load data */
  
  
  SET  @#ManualTariffAmount=NULL
  SET  @#AmountIncludesTariff=NULL
  
  SELECT @BlackListed_Accepted=BlackListed_Accepted, 
       @ClientID=ClientID, 
       @Code=Code, 
       @Command=Command, 
       @Enabled=Enabled, 
       @EndDate=EndDate, 
       @EndTime=EndTime, 
       @FrequencyType=FrequencyType, 
       @IntervalInSeconds=IntervalInSeconds, 
       @MbSchedulerID=MbSchedulerID, 
       @Name=Name, 
       @StartDate=StartDate, 
       @StartTime=StartTime, 
       @TheDate=TheDate
  FROM Documents left join DocDataAzureScheduler
    on Documents.ID=DocDataAzureScheduler.ID
  WHERE Documents.ID=@DocumentID
  
  select @CurrentMethodID=MethodID from DocEvents where ID=@EventID
  
  
  
   /* BLOCK update task */
  
  
  declare @UserNameTerm nvarchar(128) = SUSER_NAME()+' '+HOST_NAME()   
  
  
  update integrations.MbSchedulerTasks set 
    Enabled = 0,
    UpdatedOn = getdate(), UpdatedBy = @UserNameTerm 
  where MbSchedulerID = @MbSchedulerID
  IF @@ERROR<>0 RETURN 0
  
  
   /* BLOCK Exit 1 */
  
  
  RETURN 1

END
Go

if exists (select * from dbo.sysobjects where id = object_id('spdf_AzureScheduler_Enable') and OBJECTPROPERTY(id, 'IsProcedure') = 1)drop procedure spdf_AzureScheduler_Enable
Go
CREATE PROCEDURE spdf_AzureScheduler_Enable
  @DocumentID integer,
  @EventID    integer
AS
BEGIN

  --common
  DECLARE  @RC int
  --TCaseStudioLoadData
  DECLARE  @BlackListed_Accepted bit
  DECLARE  @ClientID integer
  DECLARE  @Code nvarchar(max)
  DECLARE  @Command nvarchar(max)
  DECLARE  @Enabled bit
  DECLARE  @EndDate datetime
  DECLARE  @EndTime nvarchar(max)
  DECLARE  @FrequencyType int
  DECLARE  @IntervalInSeconds int
  DECLARE  @MbSchedulerID int
  DECLARE  @Name nvarchar(max)
  DECLARE  @StartDate datetime
  DECLARE  @StartTime nvarchar(max)
  DECLARE  @TheDate datetime
  DECLARE  @#AmountIncludesTariff int
  DECLARE  @#ManualTariffAmount int
  --for block Load data
  DECLARE  @CurrentMethodID int

  
  
   /* BLOCK Load data */
  
  
  SET  @#ManualTariffAmount=NULL
  SET  @#AmountIncludesTariff=NULL
  
  SELECT @BlackListed_Accepted=BlackListed_Accepted, 
       @ClientID=ClientID, 
       @Code=Code, 
       @Command=Command, 
       @Enabled=Enabled, 
       @EndDate=EndDate, 
       @EndTime=EndTime, 
       @FrequencyType=FrequencyType, 
       @IntervalInSeconds=IntervalInSeconds, 
       @MbSchedulerID=MbSchedulerID, 
       @Name=Name, 
       @StartDate=StartDate, 
       @StartTime=StartTime, 
       @TheDate=TheDate
  FROM Documents left join DocDataAzureScheduler
    on Documents.ID=DocDataAzureScheduler.ID
  WHERE Documents.ID=@DocumentID
  
  select @CurrentMethodID=MethodID from DocEvents where ID=@EventID
  
  
  
   /* BLOCK update task */
  
  
  declare @UserNameTerm nvarchar(128) = SUSER_NAME()+' '+HOST_NAME()   
  
  
  update integrations.MbSchedulerTasks set 
    Enabled = 1,
    UpdatedOn = getdate(), UpdatedBy = @UserNameTerm 
  where MbSchedulerID = @MbSchedulerID
  IF @@ERROR<>0 RETURN 0
  
  
   /* BLOCK Exit 1 */
  
  
  RETURN 1

END
Go

if exists (select * from dbo.sysobjects where id = object_id('spdf_AzureScheduler_Edit_Disabled') and OBJECTPROPERTY(id, 'IsProcedure') = 1)drop procedure spdf_AzureScheduler_Edit_Disabled
Go
CREATE PROCEDURE spdf_AzureScheduler_Edit_Disabled
  @DocumentID integer,
  @EventID    integer
AS
BEGIN

  --common
  DECLARE  @RC int
  --TCaseStudioLoadData
  DECLARE  @BlackListed_Accepted bit
  DECLARE  @ClientID integer
  DECLARE  @Code nvarchar(max)
  DECLARE  @Command nvarchar(max)
  DECLARE  @Enabled bit
  DECLARE  @EndDate datetime
  DECLARE  @EndTime nvarchar(max)
  DECLARE  @FrequencyType int
  DECLARE  @IntervalInSeconds int
  DECLARE  @MbSchedulerID int
  DECLARE  @Name nvarchar(max)
  DECLARE  @StartDate datetime
  DECLARE  @StartTime nvarchar(max)
  DECLARE  @TheDate datetime
  DECLARE  @#AmountIncludesTariff int
  DECLARE  @#ManualTariffAmount int
  --for block Load data
  DECLARE  @CurrentMethodID int

  
  
   /* BLOCK Load data */
  
  
  SET  @#ManualTariffAmount=NULL
  SET  @#AmountIncludesTariff=NULL
  
  SELECT @BlackListed_Accepted=BlackListed_Accepted, 
       @ClientID=ClientID, 
       @Code=Code, 
       @Command=Command, 
       @Enabled=Enabled, 
       @EndDate=EndDate, 
       @EndTime=EndTime, 
       @FrequencyType=FrequencyType, 
       @IntervalInSeconds=IntervalInSeconds, 
       @MbSchedulerID=MbSchedulerID, 
       @Name=Name, 
       @StartDate=StartDate, 
       @StartTime=StartTime, 
       @TheDate=TheDate
  FROM Documents left join DocDataAzureScheduler
    on Documents.ID=DocDataAzureScheduler.ID
  WHERE Documents.ID=@DocumentID
  
  select @CurrentMethodID=MethodID from DocEvents where ID=@EventID
  
  
  
   /* BLOCK update task */
  
  
  declare @FrequencyTypeStr nvarchar(128) = case when @FrequencyType = 1 then 'EVERY-FEW-SECONDS' when @FrequencyType = 2 then 'ONCE-A-DAY' end
  declare @UserNameTerm nvarchar(128) = SUSER_NAME()+' '+HOST_NAME()   
  
  
  update integrations.MbSchedulerTasks set 
    MbSchedulerCode = @Code,
    MbSchedulerName = @Name,
    MbSchedulerCommand= @Command,
    ActiveStartDate_YYYYMMDD = convert(varchar,@StartDate,112),
    ActiveEndDate_YYYYMMDD = convert(varchar,@EndDate,112),  
    FrequencyType = @FrequencyTypeStr,
    ActiveStartTime_HHMMSS = @StartTime,
    ActiveEndTime_HHMMSS = @EndTime,  
    IntervalInSecondsBetweenRuns = @IntervalInSeconds,        
    UpdatedOn = getdate(), UpdatedBy = @UserNameTerm 
  where MbSchedulerID = @MbSchedulerID
  IF @@ERROR<>0 RETURN 0
  
  
   /* BLOCK Exit 1 */
  
  
  RETURN 1

END
Go

exec sp_GrantAll
