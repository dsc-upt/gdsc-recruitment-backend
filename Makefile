RUN_ARGS := $(wordlist 2,$(words $(MAKECMDGOALS)),$(MAKECMDGOALS))
$(eval $(RUN_ARGS):;@:)
MAIN_PROJECT := RecruitmentBackend

run: $(MAIN_PROJECT)/appsettings.Development.json
	dotnet watch run --project $(MAIN_PROJECT) --launch-profile $(MAIN_PROJECT) $(RUN_ARGS)

migrations: $(MAIN_PROJECT)/appsettings.Development.json
	dotnet ef migrations add $(RUN_ARGS) --startup-project $(MAIN_PROJECT)

migrate: $(MAIN_PROJECT)/appsettings.Development.json
	dotnet ef database update --startup-project $(MAIN_PROJECT)
