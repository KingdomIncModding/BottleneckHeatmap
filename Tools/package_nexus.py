"""Package Bottleneck Heatmap for Nexus Mods and GitHub releases."""
from pathlib import Path, PurePosixPath
import hashlib
import json
import tempfile
import xml.etree.ElementTree as ET
import zipfile

PROJECT = Path(__file__).resolve().parents[1]
GAME = PROJECT.parents[1]
OUT = PROJECT / "Releases"
VERSION = ET.parse(PROJECT / "BottleneckHeatmap.csproj").getroot().findtext("PropertyGroup/Version")


def digest(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def archive(path: Path, entries: dict):
    with zipfile.ZipFile(path, "w", compression=zipfile.ZIP_DEFLATED, compresslevel=9) as out:
        for name, data in sorted(entries.items()):
            parts = PurePosixPath(name)
            assert not parts.is_absolute() and ".." not in parts.parts and ":" not in name
            out.writestr(name, data)

    with zipfile.ZipFile(path) as check:
        assert check.testzip() is None and set(check.namelist()) == set(entries)
        with tempfile.TemporaryDirectory(prefix="PackageExtract-") as tmp:
            root = Path(tmp)
            check.extractall(root)
            extracted = {p.relative_to(root).as_posix(): p.read_bytes() for p in root.rglob("*") if p.is_file()}
            assert extracted == entries, "Extracted contents mismatch"


def main():
    dll_path = PROJECT / "bin/Release/netstandard2.1/BottleneckHeatmap.dll"
    assert dll_path.exists(), f"Missing DLL: {dll_path}"
    dll = dll_path.read_bytes()

    OUT.mkdir(exist_ok=True)
    player = {
        "BepInEx/plugins/BottleneckHeatmap/BottleneckHeatmap.dll": dll,
        "Bottleneck Heatmap README.txt": (PROJECT / "Nexus/README.txt").read_bytes(),
        "Bottleneck Heatmap REQUIREMENTS.txt": (PROJECT / "Nexus/REQUIREMENTS.txt").read_bytes(),
        "Bottleneck Heatmap CHANGELOG.txt": (PROJECT / "Nexus/CHANGELOG.txt").read_bytes(),
    }

    player_zip = OUT / f"KingdomInc-BottleneckHeatmap-{VERSION}.zip"
    archive(player_zip, player)

    result = {
        "name": "Bottleneck Heatmap",
        "version": VERSION,
        "dllSha256": digest(dll),
        "archive": str(player_zip),
        "bytes": player_zip.stat().st_size,
        "sha256": digest(player_zip.read_bytes()),
        "files": sorted(player.keys()),
    }

    record_path = OUT / "PACKAGE_VERIFICATION.json"
    record_path.write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(result, indent=2))


if __name__ == "__main__":
    main()
