VERSION ?= 2.1.0
ARTIFACTS_DIR := artifacts

RIDS := osx-x64 osx-arm64 win-x64 win-arm64 linux-x64 linux-arm64
PROJECTS := src/HspDecompiler.Gui/HspDecompiler.Gui.csproj \
            src/HspDecompiler.Cli/HspDecompiler.Cli.csproj

PUBLISH_FLAGS := -c Release \
	--self-contained true \
	-p:PublishSingleFile=true \
	-p:IncludeNativeLibrariesForSelfExtract=true \
	-p:EnableCompressionInSingleFile=true \
	-p:DebugType=none \
	-p:DebugSymbols=false

.PHONY: all build test format check clean publish $(addprefix publish-,$(RIDS))

all: build test

build:
	dotnet build HspDecompiler.sln --warnaserror

test:
	dotnet test HspDecompiler.sln --no-build --verbosity normal

format:
	dotnet format HspDecompiler.sln

check:
	dotnet format HspDecompiler.sln --verify-no-changes --verbosity diagnostic

clean:
	dotnet clean HspDecompiler.sln -v q
	rm -rf $(ARTIFACTS_DIR)

publish: $(addprefix publish-,$(RIDS))
	@echo ""
	@echo "=== Build complete ==="
	@ls -lh $(ARTIFACTS_DIR)/*.zip

define PUBLISH_TEMPLATE
publish-$(1):
	@echo "=== Publishing $(1) ==="
	@mkdir -p $(ARTIFACTS_DIR)/stage-$(1)
	@$(foreach proj,$(PROJECTS),dotnet publish $(proj) -r $(1) -o $(ARTIFACTS_DIR)/stage-$(1) $(PUBLISH_FLAGS) &&) true
	@find $(ARTIFACTS_DIR)/stage-$(1) -name '*.pdb' -delete 2>/dev/null || true
	@cd $(ARTIFACTS_DIR)/stage-$(1) && zip -r ../deHSP-$(VERSION)-$(1).zip .
	@echo "  -> $(ARTIFACTS_DIR)/deHSP-$(VERSION)-$(1).zip"
	@rm -rf $(ARTIFACTS_DIR)/stage-$(1)
endef

$(foreach rid,$(RIDS),$(eval $(call PUBLISH_TEMPLATE,$(rid))))
