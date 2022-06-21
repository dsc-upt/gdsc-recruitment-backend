RUN_ARGS := $(wordlist 2,$(words $(MAKECMDGOALS)),$(MAKECMDGOALS))
 # ...and turn them into do-nothing targets
 $(eval $(RUN_ARGS):;@:)

run: RecruitmentBackend/appsettings.Development.json
	dotnet watch run --project RecruitmentBackend --launch-profile RecruitmentBackend $(RUN_ARGS)

migrations: RecruitmentBackend/appsettings.Development.json
	dotnet ef migrations add $(RUN_ARGS) --startup-project RecruitmentBackend
