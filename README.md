# FastDLCompressor

## Introduction

FastDLCompressor is a tool to create a compressed FastDL directory for Garry's Mod.

## Usage

Open a command line window (cmd.exe) and run the compressor with your source and target directory.

    FastDLCompressor.exe -s <source> -t <target>

- \<source\> is the source directory, that should be compressed
- \<target\> is your FastDL directory, where the compressed files should be stored

**Warning:** The target directory will be deleted before compression!

### Example

    FastDLCompressor.exe -s C:\resources -t C:\fastdl

## Usage with settings.json

### settings.json

    {
        "sources": [
            {
                "dir": "C:\\first\\dir"
            },
            {
                "dir": "C:\\second\\dir"
                "includes": [
                    "materials/test(/.*)?"
                ],
                "excludes": [
                    "materials/.*"
                ]
            }
        ],
        "compression": {
            "level": 9,
            "minimumSize": 5120
        },
        "target": "C:\\target",
        "cleanupTarget": true,
        "threads": 0,
        "verbose": false
    }

The above example uses two source directories.
_C:\first\dir_ and _C:\second\dir_.

The first directory is used without any restrictions.

The second directory defines some restrictions. Includes will match first. If they match, excludes won't be matched on this path.
So in the above example only the _materials/test_ directory from _materials/_ will be included.

The target directory is set to _C:\target_ and will be deleted first, because _cleanupTarget_ is set to _true_.

The compressor will use all logical processors for compression, because _threads_ is set to 0. _threads_ can be set to any value >= 0 and indicates how many threads should be used.

Only files bigger than 5 KiB will be compressed with a compression level of 9.

### Run

    FastDLCompressor.exe -c C:\settings.json
