FROM ubuntu:20.04


# installing ssh
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates
	
	
# installing dotnet
RUN apt-get update \
    && apt-get install -y --no-install-recommends wget \
    && wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb \
    && dpkg -i /tmp/packages-microsoft-prod.deb \
    && rm /tmp/packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y apt-transport-https \
    && apt-get update \
    && apt-get install -y dotnet-runtime-3.1 \
    && rm -rf /var/lib/apt/lists/*


COPY ./init.sh /usr/local/bin/    
RUN chmod u+x /usr/local/bin/init.sh


RUN mkdir /application
ADD ./bin/Hub /application
RUN chmod +x /application/Hub
ADD ./entrypoint.sh /application
ADD ./bin/NLog.config /application
ADD ./bin/western_union/CurrencyData.csv /application


ENTRYPOINT ["/usr/local/bin/init.sh"]
