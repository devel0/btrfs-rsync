# btrfs-rsync

[![NuGet Badge](https://buildstats.info/nuget/btrfs-rsync)](https://www.nuget.org/packages/btrfs-rsync/)

BTRFS Rsync

- [Quickstart](#quickstart)
- [command line](#command-line)
- [test](#test)
- [How this project was built](#how-this-project-was-built)

<hr/>

## Quickstart

- Requirements: [Download NET Core SDK](https://dotnet.microsoft.com/download)
- Install the tool:

```sh
dotnet tool update -g btrfs-rsync
```

- if `~/.dotnet/tools` dotnet global tool isn't in path it can be added to your `~/.bashrc`

```sh
echo 'export PATH=$PATH:~/.dotnet/tools' >> ~/.bashrc
```

## command line

```sh
devel0@main:~$ btrfs-rsync
Invalid SOURCE or TARGET given

Usage: btrfs-rsync [OPTIONS] SOURCE TARGET

Synchronize btrfs SOURCE filesystem with given TARGET.
SOURCE and TARGET must mounted btrfs filesystem path.

 Mandatory:

  SOURCE                source path
  TARGET                target path

 Optional:

  --dry-run             list sync actions without apply (simulation mode)
  --skip-snap-resync    avoid resync existing subvolume snapshots

```

## how does it work

- using `--dry-run` backup approach can be viewed
- relevant code [here](https://github.com/devel0/btrfs-rsync/blob/864a5450fd3e24b0c9c7c886f2ccf0c5c69c3896/btrfs-rsync/Tool.cs#L131-L179)

## test

- Before to run execution commands that will be done can be inspected using `--dry-run` switch

```sh
devel0@main:~$ btrfs-rsync --dry-run /disk-4tb /disk-4tb-bk2

path:[/disk-4tb/backup/data/current]
uuid:[f843bf00-2ec2-a046-9beb-b1ada867201d]
parentUUID:[-]=[]
generation:[5902]
genAtCreation:[9]
children:[4]

  path:[/disk-4tb/backup/data/@GMT-2017.10.08-19.29.07]
  uuid:[1152aea4-6c9d-d84a-a051-3cd443ff9815]
  parentUUID:[f843bf00-2ec2-a046-9beb-b1ada867201d]=[/disk-4tb/backup/data/current]
  generation:[5690]
  genAtCreation:[219]
  children:[0]

  path:[/disk-4tb/backup/data/@GMT-2018.05.04-17.29.20-ARCHIVED-N01]
  uuid:[9550dfc7-346b-844f-b4e5-823ff6c5926f]
  parentUUID:[f843bf00-2ec2-a046-9beb-b1ada867201d]=[/disk-4tb/backup/data/current]
  generation:[5746]
  genAtCreation:[386]
  children:[0]

  path:[/disk-4tb/backup/data/@GMT-2018.09.04-21.29.13-ARCHIVED-N02]
  uuid:[8edc41d1-8389-eb4b-a718-203f17f50b42]
  parentUUID:[f843bf00-2ec2-a046-9beb-b1ada867201d]=[/disk-4tb/backup/data/current]
  generation:[5790]
  genAtCreation:[558]
  children:[0]

  path:[/disk-4tb/backup/data/@GMT-2019.08.03-09.58.00-ARCHIVED-N03]
  uuid:[072e0a25-0aac-1a4a-b79d-5d88c1561206]
  parentUUID:[f843bf00-2ec2-a046-9beb-b1ada867201d]=[/disk-4tb/backup/data/current]
  generation:[5873]
  genAtCreation:[797]
  children:[0]

path:[/disk-4tb/binshare]
uuid:[58508bd7-d161-6641-97ea-7a0566c0f63e]
parentUUID:[-]=[]
generation:[5901]
genAtCreation:[10]
children:[0]

path:[/disk-4tb/backup/vms/current]
uuid:[5271c20e-9cad-dc4a-839b-3a3d2a86f301]
parentUUID:[-]=[]
generation:[1033]
genAtCreation:[806]
children:[1]

  path:[/disk-4tb/backup/vms/@GMT-2019.08.04-12.53.58-ARCHIVED-N01]
  uuid:[f7120103-c144-df44-b598-376d7f2e4f68]
  parentUUID:[5271c20e-9cad-dc4a-839b-3a3d2a86f301]=[/disk-4tb/backup/vms/current]
  generation:[989]
  genAtCreation:[839]
  children:[0]

==============================================================================
WORKPLAN
==============================================================================
ensurePath: /disk-4tb/backup/data -> /disk-4tb-bk2/backup/data

createSubvol: /disk-4tb/backup/data/current -> /disk-4tb-bk2/backup/data/current

rsync: /disk-4tb/backup/data/@GMT-2017.10.08-19.29.07 -> /disk-4tb-bk2/backup/data/current

snap: /disk-4tb-bk2/backup/data/current -> /disk-4tb-bk2/backup/data/@GMT-2017.10.08-19.29.07

rsync: /disk-4tb/backup/data/@GMT-2018.05.04-17.29.20-ARCHIVED-N01 -> /disk-4tb-bk2/backup/data/current

snap: /disk-4tb-bk2/backup/data/current -> /disk-4tb-bk2/backup/data/@GMT-2018.05.04-17.29.20-ARCHIVED-N01

rsync: /disk-4tb/backup/data/@GMT-2018.09.04-21.29.13-ARCHIVED-N02 -> /disk-4tb-bk2/backup/data/current

snap: /disk-4tb-bk2/backup/data/current -> /disk-4tb-bk2/backup/data/@GMT-2018.09.04-21.29.13-ARCHIVED-N02

rsync: /disk-4tb/backup/data/@GMT-2019.08.03-09.58.00-ARCHIVED-N03 -> /disk-4tb-bk2/backup/data/current

snap: /disk-4tb-bk2/backup/data/current -> /disk-4tb-bk2/backup/data/@GMT-2019.08.03-09.58.00-ARCHIVED-N03

rsync: /disk-4tb/backup/data/current -> /disk-4tb-bk2/backup/data/current
  exclude: [/disk-4tb-bk2/backup/data/@GMT-2017.10.08-19.29.07]
  exclude: [/disk-4tb-bk2/backup/data/@GMT-2018.05.04-17.29.20-ARCHIVED-N01]
  exclude: [/disk-4tb-bk2/backup/data/@GMT-2018.09.04-21.29.13-ARCHIVED-N02]
  exclude: [/disk-4tb-bk2/backup/data/@GMT-2019.08.03-09.58.00-ARCHIVED-N03]

createSubvol: /disk-4tb/binshare -> /disk-4tb-bk2/binshare

rsync: /disk-4tb/binshare -> /disk-4tb-bk2/binshare

ensurePath: /disk-4tb/backup/vms -> /disk-4tb-bk2/backup/vms

createSubvol: /disk-4tb/backup/vms/current -> /disk-4tb-bk2/backup/vms/current

rsync: /disk-4tb/backup/vms/@GMT-2019.08.04-12.53.58-ARCHIVED-N01 -> /disk-4tb-bk2/backup/vms/current

snap: /disk-4tb-bk2/backup/vms/current -> /disk-4tb-bk2/backup/vms/@GMT-2019.08.04-12.53.58-ARCHIVED-N01

rsync: /disk-4tb/backup/vms/current -> /disk-4tb-bk2/backup/vms/current
  exclude: [/disk-4tb-bk2/backup/vms/@GMT-2019.08.04-12.53.58-ARCHIVED-N01]


==============================================================================
RUNNING
==============================================================================
mkdir -p /disk-4tb-bk2/backup/data
btrfs sub create /disk-4tb-bk2/backup/data/current
rsync -Aav --delete /disk-4tb/backup/data/@GMT-2017.10.08-19.29.07/ /disk-4tb-bk2/backup/data/current/
btrfs sub snap /disk-4tb-bk2/backup/data/current /disk-4tb-bk2/backup/data/@GMT-2017.10.08-19.29.07
rsync -Aav --delete /disk-4tb/backup/data/@GMT-2018.05.04-17.29.20-ARCHIVED-N01/ /disk-4tb-bk2/backup/data/current/
btrfs sub snap /disk-4tb-bk2/backup/data/current /disk-4tb-bk2/backup/data/@GMT-2018.05.04-17.29.20-ARCHIVED-N01
rsync -Aav --delete /disk-4tb/backup/data/@GMT-2018.09.04-21.29.13-ARCHIVED-N02/ /disk-4tb-bk2/backup/data/current/
btrfs sub snap /disk-4tb-bk2/backup/data/current /disk-4tb-bk2/backup/data/@GMT-2018.09.04-21.29.13-ARCHIVED-N02
rsync -Aav --delete /disk-4tb/backup/data/@GMT-2019.08.03-09.58.00-ARCHIVED-N03/ /disk-4tb-bk2/backup/data/current/
btrfs sub snap /disk-4tb-bk2/backup/data/current /disk-4tb-bk2/backup/data/@GMT-2019.08.03-09.58.00-ARCHIVED-N03
rsync -Aav --delete --exclude=/disk-4tb-bk2/backup/data/@GMT-2017.10.08-19.29.07 --exclude=/disk-4tb-bk2/backup/data/@GMT-2018.05.04-17.29.20-ARCHIVED-N01 --exclude=/disk-4tb-bk2/backup/data/@GMT-2018.09.04-21.29.13-ARCHIVED-N02 --exclude=/disk-4tb-bk2/backup/data/@GMT-2019.08.03-09.58.00-ARCHIVED-N03 /disk-4tb/backup/data/current/ /disk-4tb-bk2/backup/data/current/
btrfs sub create /disk-4tb-bk2/binshare
rsync -Aav --delete /disk-4tb/binshare/ /disk-4tb-bk2/binshare/
mkdir -p /disk-4tb-bk2/backup/vms
btrfs sub create /disk-4tb-bk2/backup/vms/current
rsync -Aav --delete /disk-4tb/backup/vms/@GMT-2019.08.04-12.53.58-ARCHIVED-N01/ /disk-4tb-bk2/backup/vms/current/
btrfs sub snap /disk-4tb-bk2/backup/vms/current /disk-4tb-bk2/backup/vms/@GMT-2019.08.04-12.53.58-ARCHIVED-N01
rsync -Aav --delete --exclude=/disk-4tb-bk2/backup/vms/@GMT-2019.08.04-12.53.58-ARCHIVED-N01 /disk-4tb/backup/vms/current/ /disk-4tb-bk2/backup/vms/current/
```

- a second run will only synchronize volumes

```sh
==============================================================================
RUNNING
==============================================================================
rsync -Aav --delete /disk-4tb/backup/data/@GMT-2017.10.08-19.29.07/ /disk-4tb-bk/backup/data/@GMT-2017.10.08-19.29.07/
rsync -Aav --delete /disk-4tb/backup/data/@GMT-2018.05.04-17.29.20-ARCHIVED-N01/ /disk-4tb-bk/backup/data/@GMT-2018.05.04-17.29.20-ARCHIVED-N01/
rsync -Aav --delete /disk-4tb/backup/data/@GMT-2018.09.04-21.29.13-ARCHIVED-N02/ /disk-4tb-bk/backup/data/@GMT-2018.09.04-21.29.13-ARCHIVED-N02/
rsync -Aav --delete /disk-4tb/backup/data/@GMT-2019.08.03-09.58.00-ARCHIVED-N03/ /disk-4tb-bk/backup/data/@GMT-2019.08.03-09.58.00-ARCHIVED-N03/
rsync -Aav --delete --exclude=/disk-4tb-bk/backup/data/@GMT-2017.10.08-19.29.07 --exclude=/disk-4tb-bk/backup/data/@GMT-2018.05.04-17.29.20-ARCHIVED-N01 --exclude=/disk-4tb-bk/backup/data/@GMT-2018.09.04-21.29.13-ARCHIVED-N02 --exclude=/disk-4tb-bk/backup/data/@GMT-2019.08.03-09.58.00-ARCHIVED-N03 /disk-4tb/backup/data/current/ /disk-4tb-bk/backup/data/current/
rsync -Aav --delete /disk-4tb/binshare/ /disk-4tb-bk/binshare/
rsync -Aav --delete /disk-4tb/backup/vms/@GMT-2019.08.04-12.53.58-ARCHIVED-N01/ /disk-4tb-bk/backup/vms/@GMT-2019.08.04-12.53.58-ARCHIVED-N01/
rsync -Aav --delete --exclude=/disk-4tb-bk/backup/vms/@GMT-2019.08.04-12.53.58-ARCHIVED-N01 /disk-4tb/backup/vms/current/ /disk-4tb-bk/backup/vms/current/
```

- option `--skip-sub-resync` can be specified to avoid resynchronized snapshotted subvolumes

```
==============================================================================
RUNNING
==============================================================================
rsync -Aav --delete --exclude=/disk-4tb-bk/backup/data/@GMT-2017.10.08-19.29.07 --exclude=/disk-4tb-bk/backup/data/@GMT-2018.05.04-17.29.20-ARCHIVED-N01 --exclude=/disk-4tb-bk/backup/data/@GMT-2018.09.04-21.29.13-ARCHIVED-N02 --exclude=/disk-4tb-bk/backup/data/@GMT-2019.08.03-09.58.00-ARCHIVED-N03 /disk-4tb/backup/data/current/ /disk-4tb-bk/backup/data/current/
rsync -Aav --delete /disk-4tb/binshare/ /disk-4tb-bk/binshare/
rsync -Aav --delete --exclude=/disk-4tb-bk/backup/vms/@GMT-2019.08.04-12.53.58-ARCHIVED-N01 /disk-4tb/backup/vms/current/ /disk-4tb-bk/backup/vms/current/
```

## How this project was built

```sh
mkdir btrfs-rsync
cd btrfs-rsync

dotnet new sln
dotnet new console -n btrfs-rsync

cd btrfs-rsync
dotnet add package netcore-util --version 1.0.6
cd ..

dotnet sln btrfs-rsync.sln add btrfs-rsync/btrfs-rsync.csproj
dotnet build
dotnet run --project btrfs-rsync
```
