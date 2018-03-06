# Build Realm-Rpg documentation
# Be sure to have docfx in your path!
Copy-Item README.MD Docs/index.md -Force
docfx ./Docs/docfx.json
docfx serve ./Docs/_site -p 8081