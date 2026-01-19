
declare
    @optionsXML nvarchar(max) = N'
     <request_options>
      <method>GET</method>
      <url>http://documents.worldbank.org/curated/en/290511577724289579/pdf/Weaker-Global-Outlook-Sharpens-Focus-on-Domestic-Reforms.pdf</url>
      <content_type>application/json; charset=utf-8</content_type>
     </request_options>
    ';
  
   -- и хидеры
   declare
    @headersXML nvarchar(max) = N'
     <request_headers>
     </request_headers>
    ',
   @responseData varbinary(max)
  

DECLARE @RC int  
exec @RC = integrations.[integrations_httpAnyRequest_binary] 
@optionsXML, @headersXML, null, null, @responseData output,
60
;

print '@RC='+coalesce(cast(@RC as nvarchar(max)), 'NULL')
print 'EXECUTION RESULTS: @responseData='+coalesce(cast(@responseData as nvarchar(max)), 'NULL')




go
