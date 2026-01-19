# docker integrator

1. To load new containters run ./deploy.sh
2. Check and edit files:
    mb_integrator_hub_secret.env 
    mb_western_union_integration_secret.env
	 
3. Run dockers "docker-compose up -d"
4. To stop dockers "docker-compose down"

5. Logs are in ./logs directory
6. For short logs "docker logs mb_integrator_hub" "docker logs mb_western_union_integration"
