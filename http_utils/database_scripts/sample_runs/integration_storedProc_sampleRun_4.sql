BEGIN TRANSACTION

declare
    @url nvarchar(max) = N'https://c-bitstamp-transactions-dev3.azurewebsites.net/user_transactions?offset=0&limit=100&sort=asc'
declare
    @optionsXML nvarchar(max) = N'
     <request_options>
      <method>GET</method>
      <url>'+convert(nvarchar(max), (select @url for xml path(''), type))+'</url>
      <content_type>charset=utf-8</content_type>
     </request_options>
    ';
  
   -- и хидеры
   declare
    @headersXML nvarchar(max) = N'
     <request_headers>
     </request_headers>
    ',
   @responseData nvarchar(max)
  

DECLARE @RC int  
exec @RC = integrations.[integrations_httpAnyRequest] @optionsXML, @headersXML, null, null, @responseData output,
90;

print '@RC='+coalesce(cast(@RC as nvarchar(max)), 'NULL')
print 'EXECUTION RESULTS: @responseData='+coalesce(cast(@responseData as nvarchar(max)), 'NULL')

COMMIT