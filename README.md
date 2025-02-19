## Description

Implements username/password authentication via LDAP for Windows openvpn Server

### Features

  * User authentication against LDAP.
  * Simple XML-style configuration file.
  * LDAP group-based access restrictions.
  * the plugin Can be authenticated through Active Directory.
  
## Usage

Add the following to your OpenVPN Server configuration file and adjust script-security to 3(adjusting the plugin path as required):

```
script-security 3
auth-user-pass-verify openvpn-auth-ldap.exe via-env
```
OpenVPN Client configuration added:
```
auth-user-pass
```

You need to modify the configuration file <openvpn-auth-ldap.exe.config>
```
Domain:Pass this domain verification
AccessGroup:User group allowed for openvpn access

Log: Whether to create a log(LoginLog folder)
InheritanceDetection: whether to allow inherited access
ReadGroupUser: Users with read-only access to user/group (only when InheritanceDetection is True)
Password: Password for this user (InheritanceDetection is True only)
```

## Document
  Document is only available in Chinese
  https://global.lioat.cn/2019/12/0055133.xhtml
  
Please help me translate and improve README
