#!/usr/bin/env bash
set -euo pipefail

OLD_NAME="Company.Template"
NEW_NAME="${1:-}"

if [[ -z "$NEW_NAME" ]]; then
  echo "Usage: ./scripts/rename-solution.sh New.Company.Name"
  exit 1
fi

if [[ "$NEW_NAME" == "$OLD_NAME" ]]; then
  echo "New name matches the current template name. Nothing to do."
  exit 0
fi

python3 <<'PY' "$OLD_NAME" "$NEW_NAME"
import sys
from pathlib import Path
import re

old_name = sys.argv[1]
new_name = sys.argv[2]
root = Path('.').resolve()
excluded_dirs = {'.git', '.vs', '.idea', '.vscode', 'bin', 'obj'}
text_exts = {'.sln', '.csproj', '.props', '.targets', '.cs', '.json', '.md', '.yml', '.yaml', '.ps1', '.sh', '.dockerignore', '.gitignore', '.http'}
special_names = {'Dockerfile', 'docker-compose.yml', '.env.example', 'PLAN.md'}

def should_skip(path: Path) -> bool:
    parts = set(path.relative_to(root).parts[:-1])
    return any(part in excluded_dirs for part in parts)

def should_process_file(path: Path) -> bool:
    if should_skip(path):
        return False
    return path.suffix in text_exts or path.name in special_names

for file in root.rglob('*'):
    if not file.is_file():
        continue
    if not should_process_file(file):
        continue
    try:
        content = file.read_text(encoding='utf-8')
    except UnicodeDecodeError:
        continue
    if old_name not in content:
        continue
    file.write_text(content.replace(old_name, new_name), encoding='utf-8')

for path in sorted(root.rglob('*'), key=lambda p: len(p.parts), reverse=True):
    if old_name in path.name:
        target = path.with_name(path.name.replace(old_name, new_name))
        path.rename(target)

print(f"Renamed template artifacts from '{old_name}' to '{new_name}'.")
PY
