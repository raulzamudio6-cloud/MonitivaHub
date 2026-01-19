
IF OBJECT_ID('integrations.RateQuotes', 'U') IS NOT NULL 
  DROP TABLE integrations.RateQuotes;

GO

CREATE TABLE integrations.RateQuotes 
(
    id int identity,
    rateRef varchar(255),
    quoteId varchar(255),
    createdOn datetime,
    expirationIntervalInSec int,
    sellCurrency char(3),
    sellAmount money,
    buyCurrency char(3),
    buyAmount money,
    rate float,
    isRateInverted bit,
    markup varchar(255),
	baseRate float
)

create index RatesQuotes_rateRef on integrations.RateQuotes(rateRef)

GO

create or alter procedure integrations.sp_SaveRateQuote
    @rateRef varchar(255),
    @quoteId varchar(255),
    @createdOn datetime,
    @expirationIntervalInSec int,
    @sellCurrency char(3),
    @sellAmount money,
    @buyCurrency char(3),
    @buyAmount money,
    @rate float,
    @isRateInverted bit,
    @markup varchar(255),
	@baseRate float
as
begin
  insert into integrations.RateQuotes (rateRef,quoteId,createdOn,expirationIntervalInSec,
    sellCurrency,sellAmount,buyCurrency,buyAmount,rate,isRateInverted,markup,baseRate)
  values (@rateRef,@quoteId,@createdOn,@expirationIntervalInSec,
    @sellCurrency,@sellAmount,@buyCurrency,@buyAmount,@rate,@isRateInverted,@markup,@baseRate)
end

GO

create or alter procedure integrations.sp_DeleteOldRateQuotes
as
begin
  delete from integrations.RateQuotes
  where GETUTCDATE() > dateadd(second,expirationIntervalInSec+600,createdOn)
end

GO

create or alter procedure integrations.sp_GetClientExchangeMarkup @clientID int = null, @SessionID varchar(255) = null,
  @MarkupTable nvarchar(max) output
WITH EXECUTE AS CALLER
AS
begin
   if @ClientID is null
     select @ClientID = ClientID from  SOAPSessions where sessionId = @SessionID

	declare @TariffCategoryID int
	select @TariffCategoryID = TariffCategoryID from Clients where id = @ClientID

   ;with a as (
      select  case when sc.id>0 then sc.CurrCodeChr else '*' end SellCurrency, case when bc.id>0 then bc.CurrCodeChr else '*' end  BuyCurrency
                       , p.FromAmount2,  p.FromAmount3,  p.FromAmount4,  p.FromAmount5,  p.FromAmount6,  p.FromAmount7
        , p.ProfitValue, p.ProfitValue2, p.ProfitValue3, p.ProfitValue4, p.ProfitValue5, p.ProfitValue6, p.ProfitValue7
      from CurrencyExchangeProfit p
	  left join Currency sc on sc.id = p.FromCurrencyID
	  left join Currency bc on bc.id = p.ToCurrencyId
      where p.ClientID = @ClientID or (p.ClientID = 0 and TariffCategoryID = @TariffCategoryID)
   )
   select @MarkupTable= isnull( Stuff(
                 (select  * from a for json auto) 
                  ,1,0,N'')
                 ,'[]') 

end

GO

CREATE or alter TRIGGER tg_CurrencyExchangeProfit_reset
ON
  CurrencyExchangeProfit
FOR
  INSERT,update,delete
AS
BEGIN
  insert into integrations.MbTasks (MbTaskType, MbTaskCode, MbTaskParams, MbTaskAttachments, MbSynchronousFlag,CreatedBy,Tag)
  values('ONE-TIME-REQUEST', 'rate-markup', 'clearCache', convert(varbinary(max),N'{"action":"clearCache"}'), 0, 'tg_CurrencyExchangeProfit_reset','')
END

GO

