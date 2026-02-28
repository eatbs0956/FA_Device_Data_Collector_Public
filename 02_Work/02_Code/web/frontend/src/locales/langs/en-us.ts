const local: App.I18n.Schema = {
  system: {
    title: 'Industrial Data Acquisition System',
    updateTitle: 'System Version Update Notification',
    updateContent: 'A new version of the system has been detected. Do you want to refresh the page immediately?',
    updateConfirm: 'Refresh immediately',
    updateCancel: 'Later'
  },
  common: {
    action: 'Action',
    add: 'Add',
    addSuccess: 'Add Success',
    backToHome: 'Back to home',
    batchDelete: 'Batch Delete',
    cancel: 'Cancel',
    close: 'Close',
    check: 'Check',
    expandColumn: 'Expand Column',
    columnSetting: 'Column Setting',
    config: 'Config',
    confirm: 'Confirm',
    createdAt: 'Created At',
    delete: 'Delete',
    deleteSuccess: 'Delete Success',
    confirmDelete: 'Are you sure you want to delete?',
    edit: 'Edit',
    warning: 'Warning',
    error: 'Error',
    index: 'Index',
    keywordSearch: 'Please enter keyword',
    logout: 'Logout',
    logoutConfirm: 'Are you sure you want to log out?',
    lookForward: 'Coming soon',
    modify: 'Modify',
    modifySuccess: 'Modify Success',
    noData: 'No Data',
    operate: 'Operate',
    pleaseCheckValue: 'Please check whether the value is valid',
    refresh: 'Refresh',
    reset: 'Reset',
    search: 'Search',
    switch: 'Switch',
    tip: 'Tip',
    trigger: 'Trigger',
    update: 'Update',
    updateSuccess: 'Update Success',
    userCenter: 'User Center',
    status: 'Status',
    import: 'Import',
    export: 'Export',
    pleaseSelect: 'Please select first',
    pleaseSelectData: 'Please select data first',
    enableSuccess: 'Enabled successfully',
    disableSuccess: 'Disabled successfully',
    enable: 'Enable',
    disable: 'Disable',
    yesOrNo: {
      yes: 'Yes',
      no: 'No'
    }
  },
  request: {
    logout: 'Logout user after request failed',
    logoutMsg: 'User status is invalid, please log in again',
    logoutWithModal: 'Pop up modal after request failed and then log out user',
    logoutWithModalMsg: 'User status is invalid, please log in again',
    refreshToken: 'The requested token has expired, refresh the token',
    tokenExpired: 'The requested token has expired',
    forbidden: 'You do not have permission to access this resource'
  },
  theme: {
    themeSchema: {
      title: 'Theme Schema',
      light: 'Light',
      dark: 'Dark',
      auto: 'Follow System'
    },
    grayscale: 'Grayscale',
    colourWeakness: 'Colour Weakness',
    layoutMode: {
      title: 'Layout Mode',
      vertical: 'Vertical Menu Mode',
      horizontal: 'Horizontal Menu Mode',
      'vertical-mix': 'Vertical Mix Menu Mode',
      'horizontal-mix': 'Horizontal Mix menu Mode',
      reverseHorizontalMix: 'Reverse first level menus and child level menus position'
    },
    recommendColor: 'Apply Recommended Color Algorithm',
    recommendColorDesc: 'The recommended color algorithm refers to',
    themeColor: {
      title: 'Theme Color',
      primary: 'Primary',
      info: 'Info',
      success: 'Success',
      warning: 'Warning',
      error: 'Error',
      followPrimary: 'Follow Primary'
    },
    scrollMode: {
      title: 'Scroll Mode',
      wrapper: 'Wrapper',
      content: 'Content'
    },
    page: {
      animate: 'Page Animate',
      mode: {
        title: 'Page Animate Mode',
        fade: 'Fade',
        'fade-slide': 'Slide',
        'fade-bottom': 'Fade Zoom',
        'fade-scale': 'Fade Scale',
        'zoom-fade': 'Zoom Fade',
        'zoom-out': 'Zoom Out',
        none: 'None'
      }
    },
    fixedHeaderAndTab: 'Fixed Header And Tab',
    header: {
      height: 'Header Height',
      breadcrumb: {
        visible: 'Breadcrumb Visible',
        showIcon: 'Breadcrumb Icon Visible'
      },
      multilingual: {
        visible: 'Display multilingual button'
      },
      globalSearch: {
        visible: 'Display global search button'
      }
    },
    tab: {
      visible: 'Tab Visible',
      cache: 'Tag Bar Info Cache',
      height: 'Tab Height',
      mode: {
        title: 'Tab Mode',
        chrome: 'Chrome',
        button: 'Button'
      }
    },
    sider: {
      inverted: 'Dark Sider',
      width: 'Sider Width',
      collapsedWidth: 'Sider Collapsed Width',
      mixWidth: 'Mix Sider Width',
      mixCollapsedWidth: 'Mix Sider Collapse Width',
      mixChildMenuWidth: 'Mix Child Menu Width'
    },
    footer: {
      visible: 'Footer Visible',
      fixed: 'Fixed Footer',
      height: 'Footer Height',
      right: 'Right Footer'
    },
    watermark: {
      visible: 'Watermark Full Screen Visible',
      text: 'Watermark Text',
      enableUserName: 'Enable User Name Watermark'
    },
    themeDrawerTitle: 'Theme Configuration',
    pageFunTitle: 'Page Function',
    resetCacheStrategy: {
      title: 'Reset Cache Strategy',
      close: 'Close Page',
      refresh: 'Refresh Page'
    },
    configOperation: {
      copyConfig: 'Copy Config',
      copySuccessMsg: 'Copy Success, Please replace the variable "themeSettings" in "src/theme/settings.ts"',
      resetConfig: 'Reset Config',
      resetSuccessMsg: 'Reset Success'
    }
  },
  route: {
    login: 'Login',
    403: 'No Permission',
    404: 'Page Not Found',
    500: 'Server Error',
    'iframe-page': 'Iframe',
    home: 'Home',
    device: 'Device Management',
    device_list: 'Device List',
    device_tag: 'Device Tag Management',
    device_group: 'Device Group',
    collection: 'Collection Management',
    collection_node: 'Collection Node',
    collection_task: 'Collection Task',
    monitor: 'Monitor Management',
    monitor_historical: 'Historical',
    monitor_realtime: 'Real Time Monitoring',
    monitor_statistics: 'Statistics',
    alarm: 'Alarm Management',
    alarm_history: 'Alarm History',
    alarm_realtime: 'Real Time Alarm',
    alarm_rule: 'Alarm Rule',
    manage: 'System Manage',
    manage_user: 'User Manage',
    manage_role: 'Role Manage',
    manage_menu: 'Menu Manage',
    exception: 'Exception',
    exception_403: '403',
    exception_404: '404',
    exception_500: '500'
  },
  page: {
    login: {
      common: {
        loginOrRegister: 'Login / Register',
        userNamePlaceholder: 'Please enter user name',
        phonePlaceholder: 'Please enter phone number',
        codePlaceholder: 'Please enter verification code',
        passwordPlaceholder: 'Please enter password',
        confirmPasswordPlaceholder: 'Please enter password again',
        codeLogin: 'Verification code login',
        confirm: 'Confirm',
        back: 'Back',
        validateSuccess: 'Verification passed',
        loginSuccess: 'Login successfully',
        welcomeBack: 'Welcome back, {userName} !'
      },
      pwdLogin: {
        title: 'Password Login',
        rememberMe: 'Remember me',
        forgetPassword: 'Forget password?',
        register: 'Register',
        otherAccountLogin: 'Other Account Login',
        otherLoginMode: 'Other Login Mode',
        superAdmin: 'Super Admin',
        admin: 'Admin',
        user: 'User'
      },
      codeLogin: {
        title: 'Verification Code Login',
        getCode: 'Get verification code',
        reGetCode: 'Reacquire after {time}s',
        sendCodeSuccess: 'Verification code sent successfully',
        imageCodePlaceholder: 'Please enter image verification code'
      },
      register: {
        title: 'Register',
        agreement: 'I have read and agree to',
        protocol: '《User Agreement》',
        policy: '《Privacy Policy》'
      },
      resetPwd: {
        title: 'Reset Password'
      },
      bindWeChat: {
        title: 'Bind WeChat'
      }
    },
    about: {
      title: 'About',
      introduction: `SoybeanAdmin is an elegant and powerful admin template, based on the latest front-end technology stack, including Vue3, Vite5, TypeScript, Pinia and UnoCSS. It has built-in rich theme configuration and components, strict code specifications, and an automated file routing system. In addition, it also uses the online mock data solution based on ApiFox. SoybeanAdmin provides you with a one-stop admin solution, no additional configuration, and out of the box. It is also a best practice for learning cutting-edge technologies quickly.`,
      projectInfo: {
        title: 'Project Info',
        version: 'Version',
        latestBuildTime: 'Latest Build Time',
        githubLink: 'Github Link',
        previewLink: 'Preview Link'
      },
      prdDep: 'Production Dependency',
      devDep: 'Development Dependency'
    },
    home: {
      greeting: 'Good morning, {userName}, today is another day full of vitality!'
    },
    function: {
      tab: {
        tabOperate: {
          title: 'Tab Operation',
          addTab: 'Add Tab',
          addTabDesc: 'To about page',
          closeTab: 'Close Tab',
          closeCurrentTab: 'Close Current Tab',
          closeAboutTab: 'Close "About" Tab',
          addMultiTab: 'Add Multi Tab',
          addMultiTabDesc1: 'To MultiTab page',
          addMultiTabDesc2: 'To MultiTab page(with query params)'
        },
        tabTitle: {
          title: 'Tab Title',
          changeTitle: 'Change Title',
          change: 'Change',
          resetTitle: 'Reset Title',
          reset: 'Reset'
        }
      },
      multiTab: {
        routeParam: 'Route Param',
        backTab: 'Back function_tab'
      },
      toggleAuth: {
        toggleAccount: 'Toggle Account',
        authHook: 'Auth Hook Function `hasAuth`',
        superAdminVisible: 'Super Admin Visible',
        adminVisible: 'Admin Visible',
        adminOrUserVisible: 'Admin and User Visible'
      },
      request: {
        repeatedErrorOccurOnce: 'Repeated Request Error Occurs Once',
        repeatedError: 'Repeated Request Error',
        repeatedErrorMsg1: 'Custom Request Error 1',
        repeatedErrorMsg2: 'Custom Request Error 2'
      }
    },
    alova: {
      scenes: {
        captchaSend: 'Captcha Send',
        autoRequest: 'Auto Request',
        visibilityRequestTips: 'Automatically request when switching browser window',
        pollingRequestTips: 'It will request every 3 seconds',
        networkRequestTips: 'Automatically request after network reconnecting',
        refreshTime: 'Refresh Time',
        startRequest: 'Start Request',
        stopRequest: 'Stop Request',
        requestCrossComponent: 'Request Cross Component',
        triggerAllRequest: 'Manually Trigger All Automated Requests'
      }
    },
    manage: {
      common: {
        status: {
          enable: 'Enable',
          disable: 'Disable'
        }
      },
      role: {
        title: 'Role List',
        roleName: 'Role Name',
        roleCode: 'Role Code',
        roleStatus: 'Role Status',
        roleDesc: 'Role Description',
        menuAuth: 'Menu Auth',
        buttonAuth: 'Button Auth',
        form: {
          roleName: 'Please enter role name',
          roleCode: 'Please enter role code',
          roleStatus: 'Please select role status',
          roleDesc: 'Please enter role description'
        },
        addRole: 'Add Role',
        editRole: 'Edit Role'
      },
      user: {
        title: 'User List',
        userName: 'User Name',
        userGender: 'Gender',
        nickName: 'Nick Name',
        userPhone: 'Phone Number',
        userEmail: 'Email',
        userStatus: 'User Status',
        userRole: 'User Role',
        userTypeLabel: 'User Type',
        form: {
          userName: 'Please enter user name',
          userGender: 'Please select gender',
          nickName: 'Please enter nick name',
          userPhone: 'Please enter phone number',
          userEmail: 'Please enter email',
          userStatus: 'Please select user status',
          userRole: 'Please select user role',
          userType: 'Please select user type'
        },
        addUser: 'Add User',
        editUser: 'Edit User',
        gender: {
          male: 'Male',
          female: 'Female'
        },
        userType: {
          user: 'Interactive Account',
          service: 'Service Account'
        },
        userTypeChangeConfirm: {
          toService: 'Are you sure to change user type from "Interactive Account" to "Service Account"?',
          toUser: 'Are you sure to change user type from "Service Account" to "Interactive Account"? After changing, edge nodes using this account may not work properly.'
        }
      },
      menu: {
        home: 'Home',
        title: 'Menu List',
        id: 'ID',
        parentId: 'Parent ID',
        menuType: 'Menu Type',
        menuName: 'Menu Name',
        routeName: 'Route Name',
        routePath: 'Route Path',
        pathParam: 'Path Param',
        layout: 'Layout Component',
        page: 'Page Component',
        i18nKey: 'I18n Key',
        icon: 'Icon',
        localIcon: 'Local Icon',
        iconTypeTitle: 'Icon Type',
        order: 'Order',
        constant: 'Constant',
        keepAlive: 'Keep Alive',
        href: 'Href',
        hideInMenu: 'Hide In Menu',
        activeMenu: 'Active Menu',
        multiTab: 'Multi Tab',
        fixedIndexInTab: 'Fixed Index In Tab',
        query: 'Query Params',
        button: 'Button',
        buttonCode: 'Button Code',
        buttonDesc: 'Button Desc',
        menuStatus: 'Menu Status',
        form: {
          home: 'Please select home',
          menuType: 'Please select menu type',
          menuName: 'Please enter menu name',
          routeName: 'Please enter route name',
          routePath: 'Please enter route path',
          pathParam: 'Please enter path param',
          page: 'Please select page component',
          layout: 'Please select layout component',
          i18nKey: 'Please enter i18n key',
          icon: 'Please enter iconify name',
          localIcon: 'Please enter local icon name',
          order: 'Please enter order',
          keepAlive: 'Please select whether to cache route',
          href: 'Please enter href',
          hideInMenu: 'Please select whether to hide menu',
          activeMenu: 'Please select route name of the highlighted menu',
          multiTab: 'Please select whether to support multiple tabs',
          fixedInTab: 'Please select whether to fix in the tab',
          fixedIndexInTab: 'Please enter the index fixed in the tab',
          queryKey: 'Please enter route parameter Key',
          queryValue: 'Please enter route parameter Value',
          button: 'Please select whether it is a button',
          buttonCode: 'Please enter button code',
          buttonDesc: 'Please enter button description',
          menuStatus: 'Please select menu status'
        },
        addMenu: 'Add Menu',
        editMenu: 'Edit Menu',
        addChildMenu: 'Add Child Menu',
        type: {
          directory: 'Directory',
          menu: 'Menu'
        },
        iconType: {
          iconify: 'Iconify Icon',
          local: 'Local Icon'
        }
      }
    },
    device: {
      title: 'Device List',
      deviceName: 'Device Name',
      deviceId: 'Device ID',
      description: 'Description',
      protocol: 'Protocol Type',
      protocolTypeLabel: 'Protocol Type',
      connection: 'Connection Status',
      edgeNode: 'Collection Node',
      deviceGroup: 'Device Group',
      location: 'Location',
      enabled: 'Enabled',
      tagCount: 'Tag Count',
      lastConnectedAt: 'Last Connected At',
      connectionConfig: 'Connection Config',
      protocolConfig: 'Protocol Config',
      tagsConfig: 'Tags Config',
      testConnection: 'Test Connection',
      form: {
        deviceName: 'Please enter device name',
        deviceId: 'Please enter device ID',
        description: 'Please enter description',
        protocol: 'Please select protocol type',
        edgeNode: 'Please select collection node',
        deviceGroup: 'Please select device group',
        location: 'Please enter location',
        enabled: 'Please select enabled status'
      },
      addDevice: 'Add Device',
      editDevice: 'Edit Device',
      protocolType: {
        modbusTcp: 'Modbus TCP',
        modbusRtu: 'Modbus RTU',
        opcUa: 'OPC UA',
        opcDa: 'OPC DA',
        s7: 'S7',
        bacnet: 'BACnet',
        other: 'Other'
      },
      connectionStatus: {
        connected: 'Connected',
        disconnected: 'Disconnected',
        error: 'Error',
        unknown: 'Unknown'
      },
      noEdgeNodeFound: 'Collection node not found',
      noDeviceGroupFound: 'Device group not found',
      pleaseSelectDevicesToDelete: 'Please select devices to delete',
      batchDeleteSuccess: 'Successfully deleted {count} devices',
      deviceDisabled: 'Device disabled',
      deviceEnabled: 'Device enabled',
      missingDeviceId: 'Missing device ID',
      disable: 'Disable',
      enable: 'Enable',
      connectionForm: {
        ip: 'IP Address',
        ipPlaceholder: 'e.g.: 192.168.1.100',
        port: 'Port',
        timeout: 'Timeout(ms)',
        retryCount: 'Retry Count',
        enableEncryption: 'Enable Encryption'
      },
      protocolForm: {
        // Common fields
        ip: 'IP Address',
        ipPlaceholder: 'e.g.: 192.168.1.100',
        port: 'Port',
        // Modbus TCP/RTU
        unitId: 'Unit ID',
        pollingInterval: 'Polling Interval(ms)',
        serialPort: 'Serial Port',
        serialPortPlaceholder: 'e.g.: COM1',
        baudRate: 'Baud Rate',
        dataBits: 'Data Bits',
        stopBits: 'Stop Bits',
        parity: 'Parity',
        slaveId: 'Slave ID',
        frameInterval: 'Frame Interval(ms)',
        // OPC UA
        serverUrl: 'Server URL',
        serverUrlPlaceholder: 'e.g.: opc.tcp://localhost:4840',
        securityMode: 'Security Mode',
        securityModePlaceholder: 'Please select security mode',
        securityPolicy: 'Security Policy',
        securityPolicyPlaceholder: 'Please select security policy',
        authenticationMode: 'Authentication Mode',
        authenticationModePlaceholder: 'Please select authentication mode',
        samplingInterval: 'Sampling Interval(ms)',
        // OPC UA authentication fields
        username: 'Username',
        usernamePlaceholder: 'Enter username',
        password: 'Password',
        passwordPlaceholder: 'Enter password (optional)',
        clientCertificatePath: 'Client Certificate Path',
        clientCertificatePathPlaceholder: 'e.g.: /path/to/client.crt or C:\\certs\\client.crt',
        clientPrivateKeyPath: 'Client Private Key Path',
        clientPrivateKeyPathPlaceholder: 'e.g.: /path/to/client.key or C:\\certs\\client.key',
        certificatePathWarning: 'Note: Certificate paths refer to the edge node local file system. Make sure the edge node can access these files.',
        // OPC DA
        serverName: 'OPC Server Name',
        serverNamePlaceholder: 'e.g.: Matrikon.OPC.Simulation',
        clsid: 'CLSID',
        clsidPlaceholder: 'Optional, e.g.: F8582CF2-88FB-11D0-B850-00C0F0104305',
        updateRate: 'Update Rate(ms)',
        // S7
        cpuType: 'CPU Type',
        rack: 'Rack',
        slot: 'Slot'
      }
    },
    deviceGroup: {
      title: 'Device Groups',
      tree: 'Group Tree',
      name: 'Name',
      description: 'Description',
      level: 'Level',
      sortOrder: 'Sort',
      deviceCount: 'Devices',
      childCount: 'Children',
      parent: 'Parent',
      addChild: 'Add Child',
      showAll: 'Show All',
      searchPlaceholder: 'Search by name',
      namePlaceholder: 'Enter group name',
      parentPlaceholder: 'Select parent group (leave empty for root)',
      descriptionPlaceholder: 'Enter group description',
      nameRequired: 'Please enter group name',
      nameLength: 'Name must be 1-50 characters',
      maxLevelReached: 'Max level (4) reached, cannot add child group',
      levelWarning: 'Already at level 3, child group will be the last level'
    },
    tag: {
      // Page title and list
      deviceList: 'Device List',
      tagList: 'Tag List',
      searchDevice: 'Search device',
      noDevice: 'No devices',
      tagCount: 'Tags: {count}',
      selectDeviceFirst: 'Please select a device first',
      showEnabledOnly: 'Show enabled only',

      // Tag fields
      tagId: 'Tag ID',
      tagName: 'Tag Name',
      tagAddress: 'Tag Address',
      dataTypeLabel: 'Data Type',
      unit: 'Unit',
      description: 'Description',
      accessModeLabel: 'Access Mode',
      minValue: 'Min Value',
      maxValue: 'Max Value',
      scalingFactor: 'Scaling Factor',
      offset: 'Offset',
      deadband: 'Deadband',
      enableRealtime: 'Realtime Push',
      enableRealtimeTip: 'Enable realtime data push to frontend',

      // Placeholders
      tagIdPlaceholder: 'Enter tag identifier',
      tagNamePlaceholder: 'Enter tag name',
      tagAddressPlaceholder: 'Enter tag address',
      dataTypePlaceholder: 'Select data type',
      statusPlaceholder: 'Select status',
      unitPlaceholder: 'e.g.: ℃, %',
      descriptionPlaceholder: 'Enter tag description',

      // Validation messages
      tagIdRequired: 'Tag ID is required',
      tagNameRequired: 'Tag name is required',
      dataTypeRequired: 'Data type is required',

      // Operations
      addTag: 'Add Tag',
      editTag: 'Edit Tag',
      batchEnable: 'Batch Enable',
      batchDisable: 'Batch Disable',
      confirmBatchDelete: 'Are you sure you want to delete {count} selected tags?',
      exportSuccess: 'Export successful',
      importNotImplemented: 'Import function is under development...',

      // Section titles
      basicInfo: 'Basic Information',
      addressConfig: 'Address Configuration',
      advancedConfig: 'Advanced Configuration',

      // Data types
      dataType: {
        int16: 'Int16',
        int32: 'Int32',
        int64: 'Int64',
        uint16: 'UInt16',
        uint32: 'UInt32',
        uint64: 'UInt64',
        float: 'Float',
        double: 'Double',
        boolean: 'Boolean',
        string: 'String'
      },

      // Access modes
      accessMode: {
        readOnly: 'Read Only',
        writeOnly: 'Write Only',
        readWrite: 'Read/Write'
      },

      // Modbus address configuration
      modbus: {
        functionCode: 'Function Code',
        address: 'Register Address',
        slaveId: 'Slave ID',
        quantity: 'Quantity'
      },

      // OPC UA address configuration
      opcua: {
        nodeId: 'NodeId',
        nodeIdPlaceholder: 'e.g.: ns=2;s=Channel1.Device1.Tag1',
        namespaceIndex: 'Namespace Index'
      },

      // OPC DA address configuration
      opcda: {
        itemId: 'ItemId',
        itemIdPlaceholder: 'e.g.: Channel1.Device1.Tag1'
      },

      // S7 address configuration
      s7: {
        area: 'Area',
        dbNumber: 'DB Number',
        offset: 'Byte Offset',
        bitOffset: 'Bit Offset'
      }
    },
    edgeNode: {
      title: 'Edge Node List',
      nodeName: 'Node Name',
      nodeId: 'Node ID',
      platform: 'Platform',
      version: 'Version',
      status: 'Status',
      registrationType: 'Registration Type',
      ipAddress: 'IP Address',
      port: 'Port',
      location: 'Location',
      deviceCount: 'Device Count',
      lastHeartbeat: 'Last Heartbeat',
      osInfo: 'OS Info',
      hardwareInfo: 'Hardware Info',
      installPath: 'Install Path',
      resourceLimits: 'Resource Limits',
      basicInfo: 'Basic Info',
      systemInfo: 'System Info',
      advancedConfig: 'Advanced Config',
      form: {
        nodeName: 'Please enter node name',
        nodeId: 'Please enter node ID (must match collector config)',
        status: 'Please select status',
        platform: 'Please select platform',
        location: 'Please enter deployment location',
        resourceLimits: 'Please enter resource limits config (JSON format)',
        version: 'Please enter version',
        ipAddress: 'Please enter IP address',
        installPath: 'Please enter install path',
        osInfo: 'Please enter OS info',
        hardwareInfo: 'Please enter hardware info (JSON format)'
      },
      addNode: 'Add Node',
      editNode: 'Edit Node',
      nodeStatus: {
        online: 'Online',
        offline: 'Offline',
        error: 'Error'
      },
      platformType: {
        net80: '.NET 8.0',
        net45: '.NET Framework 4.5'
      },
      registrationTypeOptions: {
        auto: 'Auto Registered',
        manual: 'Manually Added'
      },
      confirmDeleteNode: 'Are you sure to delete node "{name}"?',
      deleteNodeWithDevicesWarning:
        'This node is associated with {count} devices. After deletion, these devices will be disassociated.',
      deleteSuccess: 'Deleted successfully',
      confirmBatchDelete: 'Are you sure to delete {count} selected nodes?',
      batchDeleteSuccess: 'Successfully deleted {count} nodes',
      batchDeletePartialSuccess: 'Deletion completed: {success} succeeded, {fail} failed',
      editableFieldsNote:
        'Only node name, location and resource limits can be edited. Other info is auto-reported by collector.',
      manualNodeNote: 'After adding manually, the collector needs to use the same node ID to register and bindback.',
      manualNodeEditableNote: 'This node is manually added and not connected. All fields are editable.',
      manualNodeConnectedNote:
        'This node has connected to collector. System fields are auto-reported, only basic info is editable.',
      autoNodeEditNote: 'This node is auto-registered. System fields are auto-reported, only basic info is editable.'
    },
    collectionTask: {
      title: 'Collection Task List',
      name: 'Task Name',
      code: 'Task Code',
      description: 'Description',
      taskType: 'Task Type',
      defaultInterval: 'Collection Interval',
      cronExpression: 'Cron Expression',
      priority: 'Priority',
      status: 'Status',
      isEnabled: 'Enabled',
      effectiveFrom: 'Effective From',
      effectiveTo: 'Effective To',
      deviceCount: 'Device Count',
      devices: 'Linked Devices',
      form: {
        name: 'Please enter task name',
        code: 'Please enter task code (unique identifier)',
        description: 'Please enter task description',
        taskType: 'Please select task type',
        defaultInterval: 'Please enter collection interval (ms)',
        cronExpression: 'Please enter cron expression',
        priority: 'Please select priority',
        devices: 'Please select linked devices'
      },
      addTask: 'Add Task',
      editTask: 'Edit Task',
      taskTypeOptions: {
        periodic: 'Periodic',
        scheduled: 'Scheduled',
        eventDriven: 'Event Driven',
        hybrid: 'Hybrid'
      },
      taskStatusOptions: {
        draft: 'Draft',
        active: 'Active',
        paused: 'Paused',
        stopped: 'Stopped'
      },
      taskTypeDescription: {
        periodic: 'Collect data at fixed time intervals',
        scheduled: 'Execute collection tasks by cron expression',
        eventDriven: 'Device pushes data actively, system receives passively',
        hybrid: 'Mixed mode supporting periodic collection and event push'
      },
      confirmDeleteTask: 'Are you sure to delete task "{name}"?',
      deleteSuccess: 'Delete successfully',
      confirmBatchDelete: 'Are you sure to delete {count} selected tasks?',
      batchDeleteSuccess: 'Successfully deleted {count} tasks',
      batchDeletePartialSuccess: 'Delete completed: {success} success, {fail} failed',
      statusChangeSuccess: 'Status changed successfully',
      startTask: 'Start',
      pauseTask: 'Pause',
      stopTask: 'Stop',
      confirmStartTask: 'Are you sure to start task "{name}"?',
      confirmPauseTask: 'Are you sure to pause task "{name}"?',
      confirmStopTask: 'Are you sure to stop task "{name}"? After stopped, it cannot be restarted directly.',
      confirmEnableTask: 'Are you sure to enable task "{name}"?',
      confirmDisableTask: 'Are you sure to disable task "{name}"?',
      enableTask: 'Enable',
      disableTask: 'Disable',
      cronExpressionHelp: 'Format: second minute hour day month week, e.g.: 0 0/5 * * * ? (every 5 minutes)',
      intervalMs: 'ms',
      noDevicesSelected: 'No devices linked',
      selectDevices: 'Select Devices',
      selectedDevices: '{count} devices selected'
    },
    monitor: {
      // Realtime monitoring page
      realtime: {
        title: 'Realtime Monitoring',
        filterStatus: 'Status Filter',
        statusAll: 'All',
        statusOnline: 'Online',
        statusOffline: 'Offline',
        pause: 'Pause',
        resume: 'Resume',
        refresh: 'Refresh',
        autoRefresh: 'Auto Refresh',
        autoRefreshOff: 'OFF',
        deviceCount: 'Devices',
        noDevicesFound: 'No devices found'
      },
      // Device card
      deviceCard: {
        statusOnline: 'Online',
        statusOffline: 'Offline',
        statusError: 'Error',
        statusUnknown: 'Unknown',
        deviceId: 'Device ID',
        belongTo: 'Belongs to',
        updateTime: 'Update Time',
        keyIndicators: 'Key Indicators',
        noData: 'No data collected'
      },
      // Device detail dialog
      deviceDetail: {
        title: 'Device Details',
        deviceName: 'Device Name',
        deviceId: 'Device ID',
        status: 'Status',
        deviceType: 'Device Type',
        protocol: 'Protocol',
        location: 'Location',
        lastConnect: 'Last Connect',
        tagValues: 'Tag Values',
        tagName: 'Tag Name',
        value: 'Value',
        quality: 'Quality',
        time: 'Time',
        noData: 'No data'
      },
      // Historical data page
      historical: {
        title: 'Historical Data',
        timeRange: 'Time Range',
        aggregation: 'Aggregation',
        refresh: 'Refresh',
        last15min: 'Last 15 min',
        last1hour: 'Last 1 hour',
        last6hours: 'Last 6 hours',
        last1day: 'Last 1 day',
        last7days: 'Last 7 days',
        last30days: 'Last 30 days',
        rawData: 'Raw Data',
        avg1min: '1min Avg',
        avg5min: '5min Avg',
        avg1hour: '1hour Avg',
        selectedTags: '{count} tags selected',
        moreTags: '+{count} more',
        selectDeviceHint: 'Please select a device from the left tree',
        searchPlaceholder: 'Search device/tag',
        refreshTree: 'Refresh'
      },
      // Statistics page
      statistics: {
        title: 'Statistics Report',
        byDevice: 'By Device',
        byGroup: 'By Group',
        byNode: 'By Node',
        chart: 'Chart',
        table: 'Table',
        chartView: 'Chart',
        tableView: 'Table',
        refresh: 'Refresh',
        export: 'Export',
        noDataToExport: 'No data to export',
        dimension: 'Dimension',
        name: 'Name',
        totalPoints: 'Total Points',
        avgDevices: 'Avg Devices',
        onlineDevices: 'Online Devices',
        pointCount: 'Point Count',
        to: 'to',
        startTime: 'Start',
        endTime: 'End',
        last1Hour: 'Last 1 Hour',
        last6Hours: 'Last 6 Hours',
        last24Hours: 'Last 24 Hours',
        last7Days: 'Last 7 Days'
      }
    }
  },
  form: {
    required: 'Cannot be empty',
    userName: {
      required: 'Please enter user name',
      invalid: 'User name format is incorrect'
    },
    phone: {
      required: 'Please enter phone number',
      invalid: 'Phone number format is incorrect'
    },
    pwd: {
      required: 'Please enter password',
      invalid: 'At least 8 chars and include at least 3 types among uppercase/lowercase/digit/symbol'
    },
    confirmPwd: {
      required: 'Please enter password again',
      invalid: 'The two passwords are inconsistent'
    },
    code: {
      required: 'Please enter verification code',
      invalid: 'Verification code format is incorrect'
    },
    email: {
      required: 'Please enter email',
      invalid: 'Email format is incorrect'
    }
  },
  dropdown: {
    closeCurrent: 'Close Current',
    closeOther: 'Close Other',
    closeLeft: 'Close Left',
    closeRight: 'Close Right',
    closeAll: 'Close All'
  },
  icon: {
    themeConfig: 'Theme Configuration',
    themeSchema: 'Theme Schema',
    lang: 'Switch Language',
    fullscreen: 'Fullscreen',
    fullscreenExit: 'Exit Fullscreen',
    reload: 'Reload Page',
    collapse: 'Collapse Menu',
    expand: 'Expand Menu',
    pin: 'Pin',
    unpin: 'Unpin'
  },
  datatable: {
    itemCount: 'Total {total} items'
  }
};

export default local;
