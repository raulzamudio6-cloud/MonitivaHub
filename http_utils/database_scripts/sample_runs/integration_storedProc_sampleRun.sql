BEGIN TRANSACTION

declare
    @optionsXML nvarchar(max) = N'
     <request_options>
      <method>GET</method>
      <url>https://ya.ru</url>
      <content_type>application/json; charset=utf-8</content_type>
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