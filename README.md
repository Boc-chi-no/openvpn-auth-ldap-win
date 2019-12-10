## Description

Implements username/password authentication via LDAP for Windows openvpn Server

### Features

  * User authentication against LDAP.
  * Simple Apache-style configuration file.
  * LDAP group-based access restrictions.
  * the plugin Can be authenticated through Active Directory.
  
## Usage

Add the following to your OpenVPN configuration file and adjust script-security to 3(adjusting the plugin path as required):

```
script-security 3
auth-user-pass-verify openvpn-auth-ldap.exe via-env
```

You need to modify the configuration file <openvpn-auth-ldap.exe.config>
