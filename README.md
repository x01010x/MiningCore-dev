[![Build status](https://ci.appveyor.com/api/projects/status/xcgb9rdpc3n87gee?svg=true)](https://ci.appveyor.com/project/CheapPlayz/miningcore)
[![.NET](https://github.com/CheapPlayz/MiningCore/actions/workflows/dotnet.yml/badge.svg)](https://github.com/CheapPlayz/MiningCore/actions/workflows/dotnet.yml)
![license](https://img.shields.io/badge/license-GPL--3.0-green)

<img src="https://github.com/oliverw/miningcore/raw/master/logo.png" width="150">

### Features

- Supports clusters of pools each running individual currencies
- Ultra-low-latency, multi-threaded Stratum implementation using asynchronous I/O
- Adaptive share difficulty ("vardiff")
- PoW validation (hashing) using native code for maximum performance
- Session management for purging DDoS/flood initiated zombie workers
- Payment processing
- Banning System
- Live Stats [API](https://github.com/CheapPlayz/MiningCore/wiki/API) on Port 4000
- WebSocket streaming of notable events like Blocks found, Blocks unlocked, Payments and more
- POW (proof-of-work) & POS (proof-of-stake) support
- Detailed per-pool logging to console & filesystem
- Runs on Linux and Windows

## Support

~~Commercial support directly by the maintainer is available through [miningcore.pro](https://store.miningcore.pro).~~

For general questions visit the [Discussions Area](https://github.com/CheapPlayz/MiningCore/discussions).

Social Links:

[Telegram](https://t.me/MiningCoreDev)

## Contributions

Code contributions are very welcome and should be submitted as standard [pull requests](https://docs.github.com/en/pull-requests) (PR) based on the [`dev` branch](https://github.com/CheapPlayz/MiningCore/tree/dev).

## Building on Debian/Ubuntu

```console
git clone https://github.com/x01010x/MiningCore-dev.git
cd miningcore
```

Depending on your OS Version run either of these scripts:

```console
./build-debian-11.sh
```
or
```console
./build-ubuntu-20.04.sh
```
or
```console
./build-ubuntu-21.04.sh
```
or
```console
./build-ubuntu-22.04.sh
```

## Building on Windows

Download and install the [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)

```dosbatch
git clone https://github.com/CheapPlayz/MiningCore
cd miningcore
build-windows.bat
```

### Building in Visual Studio

- Install [Visual Studio 2022](https://www.visualstudio.com/vs/). Visual Studio Community Edition is fine.
- Open `MiningCore.sln` in Visual Studio

## Building using Docker Engine
In case you don't want to install any dependencies then you can build the app using the official Microsoft .NET SDK Docker image.

```console
git clone https://github.com/CheapPlayz/MiningCore
cd miningcore
```
Then build using Docker:

```console
docker run --rm -v $(pwd):/app -w /app mcr.microsoft.com/dotnet/sdk:6.0 /bin/bash -c 'apt update && apt install libssl-dev pkg-config libboost-all-dev libsodium-dev build-essential cmake -y --no-install-recommends && cd src/Miningcore && dotnet publish -c Release --framework net6.0 -o /app/build/'
```
It will use a Linux container, you will build a Linux executable that will not run on Windows or macOS. You can use a runtime argument (-r) to specify the type of assets that you want to publish (if they don't match the SDK container). The following examples assume you want assets that match your host operating system, and use runtime arguments to ensure that.

For macOS:

```console
docker run --rm -v $(pwd):/app -w /app mcr.microsoft.com/dotnet/sdk:6.0 /bin/bash -c 'apt update && apt install libssl-dev pkg-config libboost-all-dev libsodium-dev build-essential cmake -y --no-install-recommends && cd src/Miningcore && dotnet publish -c Release --framework net6.0 -o /app/build/ -r osx-x64 --self-contained false'
```

### Building and Running Miningcore from a container

**note** - The build scripts optimize  the build for the hardware platform the container is built on ( does it have avx for example).  If you run this container on a platform that does NOT have the same architecture you could have unexplained crashes.  YOU SHOULD BUILD THIS CONTAINER ON THE HOST YOU ARE GOING TO RUN THIS CONTAINER ON.

Commands to build container: `docker build -t <your_dockerhubid>/miningcore:v73-foo .`

The docker build assumes you are going to mount your  config file  in a volume mount.  for example:

```sh

docker run -d \
    -p 4000:4000 \
    -p 4066:4066 \
    -p 4067:4067 \
    --name mc    \
    -v `pwd`/config_prod.json:/app/config.json \
    --restart=unless-stopped \
    <your_dockerhubid>/miningcore:v73-foo

```





For Windows using Linux container:

```console
docker run --rm -v $(pwd):/app -w /app mcr.microsoft.com/dotnet/sdk:6.0 /bin/bash -c 'apt update && apt install libssl-dev pkg-config libboost-all-dev libsodium-dev build-essential cmake -y --no-install-recommends && cd src/Miningcore && dotnet publish -c Release --framework net6.0 -o /app/build/ -r win-x64 --self-contained false'
```

To delete used images and containers you can run after all:
```console
docker system prune -af
```

## Running Miningcore

### Production OS

Windows is **not** a supported production environment. Only Linux is. Please do not file issues related to running a pool on Windows. Windows topics should be posted under [discussions](https://github.com/CheapPlayz/MiningCore/discussions).

Running and developing Miningcore on Windows is of course supported.

### Database setup

Miningcore currently requires PostgreSQL 10 or higher.

Run Postgres's `psql` tool:

```console
sudo -u postgres psql
```

In `psql` execute:

```sql
CREATE ROLE miningcore WITH LOGIN ENCRYPTED PASSWORD 'your-secure-password';
CREATE DATABASE miningcore OWNER miningcore;
```

Quit `psql` with \q

Import the database schema:

```console
sudo -u postgres psql -d miningcore -f miningcore/src/Miningcore/Persistence/Postgres/Scripts/createdb.sql
```

#### Advanced setup

If you are planning to run a Multipool-Cluster, the simple setup might not perform well enough under high load. In this case you are strongly advised to use PostgreSQL 11 or higher. After performing the steps outlined in the basic setup above, perform these additional steps:

**WARNING**: The following step will delete all recorded shares. Do **NOT** do this on a production pool unless you backup your `shares` table using `pg_backup` first!

```console
sudo -u postgres psql -d miningcore -f miningcore/src/Miningcore/Persistence/Postgres/Scripts/createdb_postgresql_11_appendix.sql
```

After executing the command, your `shares` table is now a [list-partitioned table](https://www.postgresql.org/docs/11/ddl-partitioning.html) which dramatically improves query performance, since almost all database operations Miningcore performs are scoped to a certain pool.

The following step needs to performed **once for every new pool** you add to your cluster. Be sure to **replace all occurences** of `mypool1` in the statement below with the id of your pool from your Miningcore configuration file:

```sql
CREATE TABLE shares_mypool1 PARTITION OF shares FOR VALUES IN ('mypool1');
```

Once you have done this for all of your existing pools you should now restore your shares from backup.

### Configuration

Create a configuration file `config.json` as described [here](https://github.com/CheapPlayz/MiningCore/wiki/Configuration).

### Start the Pool

```console
cd build
MiningCore -c config.json
```

## Supported Currencies

Refer to [this file](https://github.com/oliverw/miningcore/blob/master/src/Miningcore/coins.json) for a complete list.

## Caveats

### Monero

- Monero's Wallet Daemon (monero-wallet-rpc) relies on HTTP digest authentication for authentication which is currently not supported by Miningcore. Therefore monero-wallet-rpc must be run with the `--disable-rpc-login` option. It is advisable to mitigate the resulting security risk by putting monero-wallet-rpc behind a reverse proxy like nginx with basic-authentication.
- Miningcore utilizes RandomX's light-mode by default which consumes only **256 MB of memory per RandomX-VM**. A modern (2021) era CPU will be able to handle ~ 50 shares per second in this mode.
- If you are running into throughput problems on your pool you can either increase the number of RandomX virtual machines in light-mode by adding `"randomXVmCount": x` to your pool configuration where x is at maximum equal to the machine's number of processor cores. Alternatively you can activate fast-mode by adding `"randomXFlagsAdd": "RANDOMX_FLAG_FULL_MEM"` to the pool configuration. Fast mode increases performance by 10x but requires roughly **3 GB of RAM per RandomX-VM**.

### ZCash

- Pools needs to be configured with both a t-addr and z-addr (new configuration property "z-address" of the pool configuration element)
- First configured zcashd daemon needs to control both the t-addr and the z-addr (have the private key)
- To increase the share processing throughput it is advisable to increase the maximum number of concurrent equihash solvers through the new configuration property "equihashMaxThreads" of the cluster configuration element. Increasing this value by one increases the peak memory consumption of the pool cluster by 1 GB.
- Miners may use both t-addresses and z-addresses when connecting to the pool

### Vertcoin

- Be sure to copy the file `verthash.dat` from your vertcoin blockchain folder to your Miningcore server
- In your Miningcore config file add this property to your vertcoin pool configuration: `"vertHashDataFile": "/path/to/verthash.dat",`

## API

Miningcore comes with an integrated REST API. Please refer to this page for instructions: [API](https://github.com/CheapPlayz/MiningCore/wiki/API)
## Running a production pool

A public production pool requires a web-frontend for your users to check their hashrate, earnings etc. Miningcore does not include such frontend but there are several community projects that can be used as starting point.

Once again, do not run a production pool on Windows! This is not a supported configuration.
## Errors

### First, try to remove/uninstall older .Net, if any, then install .Net 6
```console
echo "$(tput setaf 3)Installing .Net 6$(tput sgr0)"
sudo apt remove 'dotnet*'
sudo apt remove 'aspnetcore*'
sudo apt remove 'netstandard*'
sudo rm /etc/apt/sources.list.d/microsoft-prod.list
sudo rm /etc/apt/sources.list.d/microsoft-prod.list.save
sudo apt update
sudo apt install dotnet6
```

## Donations

To support this project you can become a [sponsor](https://github.com/sponsors/CheapPlayz) or send a donation to the following accounts:

* ETH:  `0xd9dc1a435720e285238e819d1ce945c1a58a0d2c`
* BTC:  `1ANSPbjF2Twpj3gPS9t4gxTBf4ERCFirTC`
* LTC:  `0xd9dc1a435720e285238e819d1ce945c1a58a0d2c`
* DOGE: `DJXyiFJ8xFrgZKkZMuuQxxswHvf2eGZa4P`
* ETC:  `0xd9dc1a435720e285238e819d1ce945c1a58a0d2c`
* DASH: `XeXtsYUMbRePC4CBnhMKNVG3HLvDMTc6yg`
* ZEC:  `t1dCNoj3cMwPWc2u16gBcsn4uYZeqc12JAS`
* XMR:  `47QiLPdstjM7yi7dFj5EAi6zEpASL2iwSRy3TuLutL5KGMMXz3ytGbkBU11QhqsWC8F5W5ievkhdcWtTu2qE7AGcLTq1Ucy`
* SOL: `Hzo8hW43k5qa7g11QHPqdjMzGiPh9Ceotm4Lpvtpr1TT`
