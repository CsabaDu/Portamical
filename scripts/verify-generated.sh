    #!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
INCLUDE="$ROOT/Portamical.Core/T4/SharedHelpers.ttinclude"

if [ ! -f "$INCLUDE" ]; then
  echo "Cannot find $INCLUDE"
  exit 1
fi

# Extract MaxArity value (assumes: const int MaxArity = <number>;)
MAX_ARITY=$(grep -Eo 'const int MaxArity[[:space:]]*=[[:space:]]*[0-9]+' "$INCLUDE" | grep -Eo '[0-9]+')

if [ -z "$MAX_ARITY" ]; then
  echo "Failed to read MaxArity from $INCLUDE"
  exit 1
fi

echo "MaxArity = $MAX_ARITY"

# Generated files to check (relative to repo root)
FILES=(
  "Portamical.Core/TestDataTypes/Models/General/TestData.generated.cs"
  "Portamical.Core/TestDataTypes/Models/Specialized/TestDataReturns.generated.cs"
  "Portamical.Core/TestDataTypes/Models/Specialized/TestDataThrows.generated.cs"
  "Portamical.Core/Factories/TestDataFactory.generated.cs"
)

FAIL=0

for f in "${FILES[@]}"; do
  if [ ! -f "$ROOT/$f" ]; then
    echo "Missing generated file: $f"
    FAIL=1
    continue
  fi

  echo "Checking $f ..."

  # 1) check that there is at least one occurrence of the highest Arg or Type param
  #    e.g. Arg9 or T9 depending on MaxArity
  if ! grep -q -E "Arg$MAX_ARITY[[:space:]]*[\{;=)]" "$ROOT/$f" && ! grep -q -E "T$MAX_ARITY[^\w]" "$ROOT/$f"; then
    echo "  -> Expected to find references to Arg$MAX_ARITY or T$MAX_ARITY in $f but none found."
    FAIL=1
  fi

  # 2) quick sanity: check that file contains the string 'MaxArity' anywhere (not mandatory)
  #    (optional, not treated as failure)
  if ! grep -q "MaxArity" "$ROOT/$f"; then
    echo "  -> Note: $f does not contain the literal 'MaxArity' (this is optional)."
  fi
done

if [ "$FAIL" -ne 0 ]; then
  echo
  echo "Generated files are out of sync with SharedHelpers.ttinclude (MaxArity = $MAX_ARITY)."
  echo "Please regenerate the .generated.cs files locally and include them in your PR."
  echo
  echo "In Visual Studio: Right-click each .tt file -> Run Custom Tool"
  echo "Or use your T4 CLI and regenerate the Portamical.Core templates."
  exit 2
fi

echo "OK: generated files appear consistent with MaxArity = $MAX_ARITY"