#!/usr/bin/env bash

set -e

source="${BASH_SOURCE[0]}"

# resolve $SOURCE until the file is no longer a symlink
while [[ -h $source ]]; do
  scriptroot="$( cd -P "$( dirname "$source" )" && pwd )"
  source="$(readlink "$source")"

  # if $source was a relative symlink, we need to resolve it relative to the path where 
  # the symlink file was located
  [[ $source != /* ]] && source="$scriptroot/$source"
done
scriptroot="$( cd -P "$( dirname "$source" )" && pwd)"

echo "Building this commit:"
git show --no-patch --pretty=raw HEAD

if [[ "${GITHUB_ACTIONS:-}" != "true" ]]; then
  . "$scriptroot/build.sh" --ci --restore --build --pack --publish --binaryLog "$@"
  exit 0
fi

repo_root="$( cd -P "$scriptroot/.." && pwd )"
configuration="Release"
sign_type="${DOTNET_INTERACTIVE_SIGN_TYPE:-}"
forwarded_args=()

while [[ $# -gt 0 ]]; do
  case "$1" in
    -configuration|--configuration|-c)
      if [[ $# -lt 2 ]]; then
        echo "Missing value for $1" >&2
        exit 1
      fi
      configuration="$2"
      shift 2
      ;;
    /p:SignType=*)
      sign_type="${1#*=}"
      shift
      ;;
    *)
      forwarded_args+=("$1")
      shift
      ;;
  esac
done

export NUGET_PACKAGES="$repo_root/.packages"
export DisableArcade=1

"$scriptroot/build.sh" --ci --no-dotnet

package_version=""
if [[ "${GITHUB_REF:-}" == refs/tags/v* ]]; then
  package_version="${GITHUB_REF#refs/tags/v}"
elif [[ -n "${OfficialBuildId:-}" ]]; then
  package_version="1.0.0-ci.${OfficialBuildId}"
elif [[ -n "${BUILD_BUILDNUMBER:-}" ]]; then
  package_version="1.0.0-ci.${BUILD_BUILDNUMBER}"
fi

common_props=(
  "/p:DisableArcade=1"
  "/p:ContinuousIntegrationBuild=true"
  "/p:PackageOutputPath=$repo_root/artifacts/packages/$configuration/Shipping"
)
if [[ -n "$package_version" ]]; then
  common_props+=("/p:PackageVersion=$package_version")
fi
if [[ -n "$sign_type" ]]; then
  common_props+=("/p:SignType=$sign_type")
fi

mkdir -p "$repo_root/artifacts/log/$configuration"

dotnet restore "$repo_root/dotnet-interactive.sln" \
  "${common_props[@]}" \
  "${forwarded_args[@]}"

dotnet build "$repo_root/dotnet-interactive.sln" \
  -c "$configuration" \
  --no-restore \
  "/bl:$repo_root/artifacts/log/$configuration/Build.binlog" \
  "${common_props[@]}" \
  "${forwarded_args[@]}"

dotnet pack "$repo_root/dotnet-interactive.sln" \
  -c "$configuration" \
  --no-build \
  --no-restore \
  "/p:BuildProjectReferences=false" \
  "${common_props[@]}" \
  "${forwarded_args[@]}"
