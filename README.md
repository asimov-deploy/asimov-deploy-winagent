Asimov Deploy WinAgent
================

Asimov Deploy consists of two major components the [Deploy Web UI](https://github.com/asimov-deploy) and deploy agents. This repository contains the windows deploy agent.

## Windows deploy agent features
* HTTP API
* Talks to Deploy UI through HTTP
* Deploy Windows Services
* Start & Stop Windows Services
* Deploy IIS Web Applications or Sites
* Start & Stop IIS Application Pools
* Execute Powershell scripts included in deploy package
* Automate load balancer (enable / disable machine in load balancer)
	* Currently only supports Alteon, but support for more is on the way.
* Execute phantomjs (& casper.js) verify & web site warmup scripts (and report progress & status to web UI)
* Deploy logs & history
* Package management
	* Hard drive / fileshare zip files (version / branch / commit can be extracted from filename)
	* Nuget support (not completed yet)
* Automatic update of deploy agent
* Automatic update of deploy agent config

## Bugs and Feedback
For bugs, questions and discussions please use the [Github Issues](asimov-deploy/issues).

## LICENSE
Copyright 2013 Ebay Inc.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

<http://www.apache.org/licenses/LICENSE-2.0>

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.