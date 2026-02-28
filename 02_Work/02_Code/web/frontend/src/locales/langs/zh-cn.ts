const local: App.I18n.Schema = {
  system: {
    title: '工业数据采集系统',
    updateTitle: '系统版本更新通知',
    updateContent: '检测到系统有新版本发布，是否立即刷新页面？',
    updateConfirm: '立即刷新',
    updateCancel: '稍后再说'
  },
  common: {
    action: '操作',
    add: '新增',
    addSuccess: '添加成功',
    backToHome: '返回首页',
    batchDelete: '批量删除',
    cancel: '取消',
    close: '关闭',
    check: '勾选',
    expandColumn: '展开列',
    columnSetting: '列设置',
    config: '配置',
    confirm: '确认',
    createdAt: '创建时间',
    delete: '删除',
    deleteSuccess: '删除成功',
    confirmDelete: '确认删除吗？',
    edit: '编辑',
    warning: '警告',
    error: '错误',
    index: '序号',
    keywordSearch: '请输入关键词搜索',
    logout: '退出登录',
    logoutConfirm: '确认退出登录吗？',
    lookForward: '敬请期待',
    modify: '修改',
    modifySuccess: '修改成功',
    noData: '无数据',
    operate: '操作',
    pleaseCheckValue: '请检查输入的值是否合法',
    refresh: '刷新',
    reset: '重置',
    search: '搜索',
    switch: '切换',
    tip: '提示',
    trigger: '触发',
    update: '更新',
    updateSuccess: '更新成功',
    userCenter: '个人中心',
    status: '状态',
    import: '导入',
    export: '导出',
    pleaseSelect: '请先选择',
    pleaseSelectData: '请先选择数据',
    enableSuccess: '启用成功',
    disableSuccess: '禁用成功',
    enable: '启用',
    disable: '禁用',
    yesOrNo: {
      yes: '是',
      no: '否'
    }
  },
  request: {
    logout: '请求失败后登出用户',
    logoutMsg: '用户状态失效，请重新登录',
    logoutWithModal: '请求失败后弹出模态框再登出用户',
    logoutWithModalMsg: '用户状态失效，请重新登录',
    refreshToken: '请求的token已过期，刷新token',
    tokenExpired: 'token已过期',
    forbidden: '您没有权限访问此资源'
  },
  theme: {
    themeSchema: {
      title: '主题模式',
      light: '亮色模式',
      dark: '暗黑模式',
      auto: '跟随系统'
    },
    grayscale: '灰色模式',
    colourWeakness: '色弱模式',
    layoutMode: {
      title: '布局模式',
      vertical: '左侧菜单模式',
      'vertical-mix': '左侧菜单混合模式',
      horizontal: '顶部菜单模式',
      'horizontal-mix': '顶部菜单混合模式',
      reverseHorizontalMix: '一级菜单与子级菜单位置反转'
    },
    recommendColor: '应用推荐算法的颜色',
    recommendColorDesc: '推荐颜色的算法参照',
    themeColor: {
      title: '主题颜色',
      primary: '主色',
      info: '信息色',
      success: '成功色',
      warning: '警告色',
      error: '错误色',
      followPrimary: '跟随主色'
    },
    scrollMode: {
      title: '滚动模式',
      wrapper: '外层滚动',
      content: '主体滚动'
    },
    page: {
      animate: '页面切换动画',
      mode: {
        title: '页面切换动画类型',
        'fade-slide': '滑动',
        fade: '淡入淡出',
        'fade-bottom': '底部消退',
        'fade-scale': '缩放消退',
        'zoom-fade': '渐变',
        'zoom-out': '闪现',
        none: '无'
      }
    },
    fixedHeaderAndTab: '固定头部和标签栏',
    header: {
      height: '头部高度',
      breadcrumb: {
        visible: '显示面包屑',
        showIcon: '显示面包屑图标'
      },
      multilingual: {
        visible: '显示多语言按钮'
      },
      globalSearch: {
        visible: '显示全局搜索按钮'
      }
    },
    tab: {
      visible: '显示标签栏',
      cache: '标签栏信息缓存',
      height: '标签栏高度',
      mode: {
        title: '标签栏风格',
        chrome: '谷歌风格',
        button: '按钮风格'
      }
    },
    sider: {
      inverted: '深色侧边栏',
      width: '侧边栏宽度',
      collapsedWidth: '侧边栏折叠宽度',
      mixWidth: '混合布局侧边栏宽度',
      mixCollapsedWidth: '混合布局侧边栏折叠宽度',
      mixChildMenuWidth: '混合布局子菜单宽度'
    },
    footer: {
      visible: '显示底部',
      fixed: '固定底部',
      height: '底部高度',
      right: '底部局右'
    },
    watermark: {
      visible: '显示全屏水印',
      text: '水印文本',
      enableUserName: '启用用户名水印'
    },
    themeDrawerTitle: '主题配置',
    pageFunTitle: '页面功能',
    resetCacheStrategy: {
      title: '重置缓存策略',
      close: '关闭页面',
      refresh: '刷新页面'
    },
    configOperation: {
      copyConfig: '复制配置',
      copySuccessMsg: '复制成功，请替换 src/theme/settings.ts 中的变量 themeSettings',
      resetConfig: '重置配置',
      resetSuccessMsg: '重置成功'
    }
  },
  route: {
    login: '登录',
    403: '无权限',
    404: '页面不存在',
    500: '服务器错误',
    'iframe-page': '外链页面',
    home: '首页',
    device: '设备管理',
    device_list: '设备列表',
    device_tag: '设备标签',
    device_group: '设备分组',
    collection: '采集管理',
    collection_node: '采集节点',
    collection_task: '采集任务',
    monitor: '监控管理',
    monitor_realtime: '实时监控',
    monitor_historical: '历史数据',
    monitor_statistics: '统计报表',
    alarm: '告警管理',
    alarm_realtime: '实时告警',
    alarm_history: '告警历史',
    alarm_rule: '告警规则',
    manage: '系统管理',
    manage_user: '用户管理',
    manage_role: '角色管理',
    manage_menu: '菜单管理',
    exception: '异常页',
    exception_403: '403',
    exception_404: '404',
    exception_500: '500'
  },
  page: {
    login: {
      common: {
        loginOrRegister: '登录 / 注册',
        userNamePlaceholder: '请输入用户名',
        phonePlaceholder: '请输入手机号',
        codePlaceholder: '请输入验证码',
        passwordPlaceholder: '请输入密码',
        confirmPasswordPlaceholder: '请再次输入密码',
        codeLogin: '验证码登录',
        confirm: '确定',
        back: '返回',
        validateSuccess: '验证成功',
        loginSuccess: '登录成功',
        welcomeBack: '欢迎回来，{userName} ！'
      },
      pwdLogin: {
        title: '密码登录',
        rememberMe: '记住我',
        forgetPassword: '忘记密码？',
        register: '注册账号',
        otherAccountLogin: '其他账号登录',
        otherLoginMode: '其他登录方式',
        superAdmin: '超级管理员',
        admin: '管理员',
        user: '普通用户'
      },
      codeLogin: {
        title: '验证码登录',
        getCode: '获取验证码',
        reGetCode: '{time}秒后重新获取',
        sendCodeSuccess: '验证码发送成功',
        imageCodePlaceholder: '请输入图片验证码'
      },
      register: {
        title: '注册账号',
        agreement: '我已经仔细阅读并接受',
        protocol: '《用户协议》',
        policy: '《隐私权政策》'
      },
      resetPwd: {
        title: '重置密码'
      },
      bindWeChat: {
        title: '绑定微信'
      }
    },
    about: {
      title: '关于',
      introduction: `SoybeanAdmin 是一个优雅且功能强大的后台管理模板，基于最新的前端技术栈，包括 Vue3, Vite5, TypeScript, Pinia 和 UnoCSS。它内置了丰富的主题配置和组件，代码规范严谨，实现了自动化的文件路由系统。此外，它还采用了基于 ApiFox 的在线Mock数据方案。SoybeanAdmin 为您提供了一站式的后台管理解决方案，无需额外配置，开箱即用。同样是一个快速学习前沿技术的最佳实践。`,
      projectInfo: {
        title: '项目信息',
        version: '版本',
        latestBuildTime: '最新构建时间',
        githubLink: 'Github 地址',
        previewLink: '预览地址'
      },
      prdDep: '生产依赖',
      devDep: '开发依赖'
    },
    home: {
      greeting: '早安，{userName}, 今天又是充满活力的一天!'
    },
    function: {
      tab: {
        tabOperate: {
          title: '标签页操作',
          addTab: '添加标签页',
          addTabDesc: '跳转到关于页面',
          closeTab: '关闭标签页',
          closeCurrentTab: '关闭当前标签页',
          closeAboutTab: '关闭"关于"标签页',
          addMultiTab: '添加多标签页',
          addMultiTabDesc1: '跳转到多标签页页面',
          addMultiTabDesc2: '跳转到多标签页页面(带有查询参数)'
        },
        tabTitle: {
          title: '标签页标题',
          changeTitle: '修改标题',
          change: '修改',
          resetTitle: '重置标题',
          reset: '重置'
        }
      },
      multiTab: {
        routeParam: '路由参数',
        backTab: '返回 function_tab'
      },
      toggleAuth: {
        toggleAccount: '切换账号',
        authHook: '权限钩子函数 `hasAuth`',
        superAdminVisible: '超级管理员可见',
        adminVisible: '管理员可见',
        adminOrUserVisible: '管理员和用户可见'
      },
      request: {
        repeatedErrorOccurOnce: '重复请求错误只出现一次',
        repeatedError: '重复请求错误',
        repeatedErrorMsg1: '自定义请求错误 1',
        repeatedErrorMsg2: '自定义请求错误 2'
      }
    },
    alova: {
      scenes: {
        captchaSend: '发送验证码',
        autoRequest: '自动请求',
        visibilityRequestTips: '浏览器窗口切换自动请求数据',
        pollingRequestTips: '每3秒自动请求一次',
        networkRequestTips: '网络重连后自动请求',
        refreshTime: '更新时间',
        startRequest: '开始请求',
        stopRequest: '停止请求',
        requestCrossComponent: '跨组件触发请求',
        triggerAllRequest: '手动触发所有自动请求'
      }
    },
    manage: {
      common: {
        status: {
          enable: '启用',
          disable: '禁用'
        }
      },
      role: {
        title: '角色列表',
        roleName: '角色名称',
        roleCode: '角色编码',
        roleStatus: '角色状态',
        roleDesc: '角色描述',
        menuAuth: '菜单权限',
        buttonAuth: '按钮权限',
        form: {
          roleName: '请输入角色名称',
          roleCode: '请输入角色编码',
          roleStatus: '请选择角色状态',
          roleDesc: '请输入角色描述'
        },
        addRole: '新增角色',
        editRole: '编辑角色'
      },
      user: {
        title: '用户列表',
        userName: '用户名',
        userGender: '性别',
        nickName: '昵称',
        userPhone: '手机号',
        userEmail: '邮箱',
        userStatus: '用户状态',
        userRole: '用户角色',
        userTypeLabel: '用户类型',
        form: {
          userName: '请输入用户名',
          userGender: '请选择性别',
          nickName: '请输入昵称',
          userPhone: '请输入手机号',
          userEmail: '请输入邮箱',
          userStatus: '请选择用户状态',
          userRole: '请选择用户角色',
          userType: '请选择用户类型'
        },
        addUser: '新增用户',
        editUser: '编辑用户',
        gender: {
          male: '男',
          female: '女'
        },
        userType: {
          user: '交互账号',
          service: '服务账号'
        },
        userTypeChangeConfirm: {
          toService: '确定将用户类型从"交互账号"改为"服务账号"吗？',
          toUser: '确定将用户类型从"服务账号"改为"交互账号"吗？服务账号改为交互账号后，使用该账号的采集节点可能无法正常工作。'
        }
      },
      menu: {
        home: '首页',
        title: '菜单列表',
        id: 'ID',
        parentId: '父级菜单ID',
        menuType: '菜单类型',
        menuName: '菜单名称',
        routeName: '路由名称',
        routePath: '路由路径',
        pathParam: '路径参数',
        layout: '布局',
        page: '页面组件',
        i18nKey: '国际化key',
        icon: '图标',
        localIcon: '本地图标',
        iconTypeTitle: '图标类型',
        order: '排序',
        constant: '常量路由',
        keepAlive: '缓存路由',
        href: '外链',
        hideInMenu: '隐藏菜单',
        activeMenu: '高亮的菜单',
        multiTab: '支持多页签',
        fixedIndexInTab: '固定在页签中的序号',
        query: '路由参数',
        button: '按钮',
        buttonCode: '按钮编码',
        buttonDesc: '按钮描述',
        menuStatus: '菜单状态',
        form: {
          home: '请选择首页',
          menuType: '请选择菜单类型',
          menuName: '请输入菜单名称',
          routeName: '请输入路由名称',
          routePath: '请输入路由路径',
          pathParam: '请输入路径参数',
          page: '请选择页面组件',
          layout: '请选择布局组件',
          i18nKey: '请输入国际化key',
          icon: '请输入图标',
          localIcon: '请选择本地图标',
          order: '请输入排序',
          keepAlive: '请选择是否缓存路由',
          href: '请输入外链',
          hideInMenu: '请选择是否隐藏菜单',
          activeMenu: '请选择高亮的菜单的路由名称',
          multiTab: '请选择是否支持多标签',
          fixedInTab: '请选择是否固定在页签中',
          fixedIndexInTab: '请输入固定在页签中的序号',
          queryKey: '请输入路由参数Key',
          queryValue: '请输入路由参数Value',
          button: '请选择是否按钮',
          buttonCode: '请输入按钮编码',
          buttonDesc: '请输入按钮描述',
          menuStatus: '请选择菜单状态'
        },
        addMenu: '新增菜单',
        editMenu: '编辑菜单',
        addChildMenu: '新增子菜单',
        type: {
          directory: '目录',
          menu: '菜单'
        },
        iconType: {
          iconify: 'iconify图标',
          local: '本地图标'
        }
      }
    },
    device: {
      title: '设备列表',
      deviceName: '设备名称',
      deviceId: '设备ID',
      description: '设备描述',
      protocol: '协议类型',
      protocolTypeLabel: '协议类型',
      connection: '连接状态',
      edgeNode: '采集节点',
      deviceGroup: '设备分组',
      location: '设备位置',
      enabled: '启用状态',
      tagCount: '标签数量',
      lastConnectedAt: '最后连接时间',
      connectionConfig: '连接配置',
      protocolConfig: '协议配置',
      tagsConfig: '标签配置',
      testConnection: '测试连接',
      form: {
        deviceName: '请输入设备名称',
        deviceId: '请输入设备ID',
        description: '请输入设备描述',
        protocol: '请选择协议类型',
        edgeNode: '请选择采集节点',
        deviceGroup: '请选择设备分组',
        location: '请输入设备位置',
        enabled: '请选择启用状态'
      },
      addDevice: '新增设备',
      editDevice: '编辑设备',
      protocolType: {
        modbusTcp: 'Modbus TCP',
        modbusRtu: 'Modbus RTU',
        opcUa: 'OPC UA',
        opcDa: 'OPC DA',
        s7: 'S7',
        bacnet: 'BACnet',
        other: '其他'
      },
      connectionStatus: {
        connected: '已连接',
        disconnected: '已断开',
        error: '错误',
        unknown: '未知'
      },
      noEdgeNodeFound: '没有查询到采集节点',
      noDeviceGroupFound: '没有查询到设备分组',
      pleaseSelectDevicesToDelete: '请选择要删除的设备',
      batchDeleteSuccess: '成功删除 {count} 个设备',
      deviceDisabled: '设备已禁用',
      deviceEnabled: '设备已启用',
      missingDeviceId: '缺少设备ID',
      disable: '禁用',
      enable: '启用',
      connectionForm: {
        ip: 'IP地址',
        ipPlaceholder: '例如: 192.168.1.100',
        port: '端口',
        timeout: '超时(ms)',
        retryCount: '重试次数',
        enableEncryption: '启用加密'
      },
      protocolForm: {
        // 通用字段
        ip: 'IP地址',
        ipPlaceholder: '例如: 192.168.1.100',
        port: '端口',
        // Modbus TCP/RTU
        unitId: '从站地址(Unit ID)',
        pollingInterval: '轮询间隔(ms)',
        serialPort: '串口',
        serialPortPlaceholder: '例如: COM1',
        baudRate: '波特率',
        dataBits: '数据位',
        stopBits: '停止位',
        parity: '校验位',
        slaveId: '从站地址(Slave ID)',
        frameInterval: '帧间隔(ms)',
        // OPC UA
        serverUrl: '服务器URL',
        serverUrlPlaceholder: '例如: opc.tcp://localhost:4840',
        securityMode: '安全模式',
        securityModePlaceholder: '请选择安全模式',
        securityPolicy: '安全策略',
        securityPolicyPlaceholder: '请选择安全策略',
        authenticationMode: '认证模式',
        authenticationModePlaceholder: '请选择认证模式',
        samplingInterval: '采样间隔(ms)',
        // OPC UA 认证字段
        username: '用户名',
        usernamePlaceholder: '请输入用户名',
        password: '密码',
        passwordPlaceholder: '请输入密码（可选）',
        clientCertificatePath: '客户端证书路径',
        clientCertificatePathPlaceholder: '例如: /path/to/client.crt 或 C:\\certs\\client.crt',
        clientPrivateKeyPath: '客户端私钥路径',
        clientPrivateKeyPathPlaceholder: '例如: /path/to/client.key 或 C:\\certs\\client.key',
        certificatePathWarning: '注意：证书路径是边缘节点本地文件系统的路径，请确保边缘节点可以访问这些文件。',
        // OPC DA
        serverName: 'OPC服务器名称',
        serverNamePlaceholder: '例如: Matrikon.OPC.Simulation',
        clsid: 'CLSID',
        clsidPlaceholder: '可选，例如: F8582CF2-88FB-11D0-B850-00C0F0104305',
        updateRate: '更新频率(ms)',
        // S7
        cpuType: 'CPU类型',
        rack: '机架号',
        slot: '槽号'
      }
    },
    deviceGroup: {
      title: '设备分组',
      tree: '分组树',
      name: '分组名称',
      description: '分组描述',
      level: '层级',
      sortOrder: '排序',
      deviceCount: '设备数',
      childCount: '子分组数',
      parent: '父分组',
      addChild: '添加子分组',
      showAll: '显示全部',
      searchPlaceholder: '输入名称搜索',
      namePlaceholder: '请输入分组名称',
      parentPlaceholder: '请选择父分组（留空为顶级分组）',
      descriptionPlaceholder: '请输入分组描述',
      nameRequired: '请输入分组名称',
      nameLength: '名称长度为1-50个字符',
      maxLevelReached: '已达最大层级限制（4级），无法添加子分组',
      levelWarning: '当前已达第3级，子分组将是最后一级'
    },
    tag: {
      // 页面标题和列表
      deviceList: '设备列表',
      tagList: '标签列表',
      searchDevice: '搜索设备',
      noDevice: '暂无设备',
      tagCount: '标签: {count}',
      selectDeviceFirst: '请先选择设备',
      showEnabledOnly: '仅显示启用设备',

      // 标签字段
      tagId: '标签标识符',
      tagName: '标签名称',
      tagAddress: '标签地址',
      dataTypeLabel: '数据类型',
      unit: '单位',
      description: '描述',
      accessModeLabel: '访问模式',
      minValue: '最小值',
      maxValue: '最大值',
      scalingFactor: '比例因子',
      offset: '偏移量',
      deadband: '死区值',
      enableRealtime: '实时推送',
      enableRealtimeTip: '启用后将实时推送数据到前端',

      // 占位符
      tagIdPlaceholder: '请输入标签标识符',
      tagNamePlaceholder: '请输入标签名称',
      tagAddressPlaceholder: '请输入标签地址',
      dataTypePlaceholder: '请选择数据类型',
      statusPlaceholder: '请选择状态',
      unitPlaceholder: '例如: ℃, %',
      descriptionPlaceholder: '请输入标签描述',

      // 验证消息
      tagIdRequired: '请输入标签标识符',
      tagNameRequired: '请输入标签名称',
      dataTypeRequired: '请选择数据类型',

      // 操作
      addTag: '新增标签',
      editTag: '编辑标签',
      batchEnable: '批量启用',
      batchDisable: '批量禁用',
      confirmBatchDelete: '确认删除选中的 {count} 个标签吗？',
      exportSuccess: '导出成功',
      importNotImplemented: '导入功能开发中...',

      // 区域标题
      basicInfo: '基本信息',
      addressConfig: '地址配置',
      advancedConfig: '高级配置',

      // 数据类型
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

      // 访问模式
      accessMode: {
        readOnly: '只读',
        writeOnly: '只写',
        readWrite: '读写'
      },

      // Modbus 地址配置
      modbus: {
        functionCode: '功能码',
        address: '寄存器地址',
        slaveId: '从站地址',
        quantity: '寄存器数量'
      },

      // OPC UA 地址配置
      opcua: {
        nodeId: 'NodeId',
        nodeIdPlaceholder: '例如: ns=2;s=Channel1.Device1.Tag1',
        namespaceIndex: '命名空间索引'
      },

      // OPC DA 地址配置
      opcda: {
        itemId: 'ItemId',
        itemIdPlaceholder: '例如: Channel1.Device1.Tag1'
      },

      // S7 地址配置
      s7: {
        area: '区域',
        dbNumber: 'DB号',
        offset: '字节偏移',
        bitOffset: '位偏移'
      }
    },
    edgeNode: {
      title: '采集节点列表',
      nodeName: '节点名称',
      nodeId: '节点ID',
      platform: '平台类型',
      version: '版本',
      status: '状态',
      registrationType: '注册方式',
      ipAddress: 'IP地址',
      port: '端口',
      location: '位置',
      deviceCount: '设备数量',
      lastHeartbeat: '最后心跳',
      osInfo: '操作系统',
      hardwareInfo: '硬件信息',
      installPath: '安装路径',
      resourceLimits: '资源限制',
      basicInfo: '基本信息',
      systemInfo: '系统信息',
      advancedConfig: '高级配置',
      form: {
        nodeName: '请输入节点名称',
        nodeId: '请输入节点ID（需与采集程序配置一致）',
        status: '请选择状态',
        platform: '请选择平台类型',
        location: '请输入部署位置',
        resourceLimits: '请输入资源限制配置(JSON格式)',
        version: '请输入版本号',
        ipAddress: '请输入IP地址',
        installPath: '请输入安装路径',
        osInfo: '请输入操作系统信息',
        hardwareInfo: '请输入硬件信息(JSON格式)'
      },
      addNode: '新增节点',
      editNode: '编辑节点',
      nodeStatus: {
        online: '在线',
        offline: '离线',
        error: '错误'
      },
      platformType: {
        net80: '.NET 8.0',
        net45: '.NET Framework 4.5'
      },
      registrationTypeOptions: {
        auto: '自动注册',
        manual: '手动添加'
      },
      confirmDeleteNode: '确认删除节点 "{name}" 吗？',
      deleteNodeWithDevicesWarning: '该节点关联了 {count} 个设备，删除后这些设备将取消关联采集节点。',
      deleteSuccess: '删除成功',
      confirmBatchDelete: '确认删除选中的 {count} 个节点吗？',
      batchDeleteSuccess: '成功删除 {count} 个节点',
      batchDeletePartialSuccess: '删除完成：成功 {success} 个，失败 {fail} 个',
      editableFieldsNote: '仅可编辑节点名称、位置和资源限制，其他信息由采集程序自动上报。',
      manualNodeNote: '手动添加节点后，采集程序启动时需使用相同的节点ID进行注册绑定。',
      manualNodeEditableNote: '该节点为手动添加且未连接采集程序，所有字段均可编辑。',
      manualNodeConnectedNote: '该节点已连接过采集程序，系统字段由程序自动上报，仅可编辑基本信息。',
      autoNodeEditNote: '该节点为自动注册，系统字段由采集程序自动上报，仅可编辑基本信息。'
    },
    collectionTask: {
      title: '采集任务列表',
      name: '任务名称',
      code: '任务编码',
      description: '任务描述',
      taskType: '任务类型',
      defaultInterval: '采集间隔',
      cronExpression: 'Cron表达式',
      priority: '优先级',
      status: '状态',
      isEnabled: '启用状态',
      effectiveFrom: '生效开始时间',
      effectiveTo: '生效结束时间',
      deviceCount: '关联设备数',
      devices: '关联设备',
      form: {
        name: '请输入任务名称',
        code: '请输入任务编码（唯一标识）',
        description: '请输入任务描述',
        taskType: '请选择任务类型',
        defaultInterval: '请输入采集间隔（毫秒）',
        cronExpression: '请输入Cron表达式',
        priority: '请选择优先级',
        devices: '请选择关联设备'
      },
      addTask: '新增任务',
      editTask: '编辑任务',
      taskTypeOptions: {
        periodic: '周期采集',
        scheduled: '定时采集',
        eventDriven: '事件驱动',
        hybrid: '混合模式'
      },
      taskStatusOptions: {
        draft: '草稿',
        active: '运行中',
        paused: '已暂停',
        stopped: '已停止'
      },
      taskTypeDescription: {
        periodic: '按固定时间间隔周期性采集数据',
        scheduled: '按Cron表达式定时执行采集任务',
        eventDriven: '由设备主动推送数据，系统被动接收',
        hybrid: '支持周期采集和事件推送的混合模式'
      },
      confirmDeleteTask: '确认删除任务 "{name}" 吗？',
      deleteSuccess: '删除成功',
      confirmBatchDelete: '确认删除选中的 {count} 个任务吗？',
      batchDeleteSuccess: '成功删除 {count} 个任务',
      batchDeletePartialSuccess: '删除完成：成功 {success} 个，失败 {fail} 个',
      statusChangeSuccess: '状态变更成功',
      startTask: '启动',
      pauseTask: '暂停',
      stopTask: '停止',
      confirmStartTask: '确认启动任务 "{name}" 吗？',
      confirmPauseTask: '确认暂停任务 "{name}" 吗？',
      confirmStopTask: '确认停止任务 "{name}" 吗？停止后无法直接重新启动，需要重新激活。',
      confirmEnableTask: '确认启用任务 "{name}" 吗？',
      confirmDisableTask: '确认禁用任务 "{name}" 吗？',
      enableTask: '启用',
      disableTask: '禁用',
      cronExpressionHelp: '格式: 秒 分 时 日 月 周，例如: 0 0/5 * * * ? (每5分钟执行)',
      intervalMs: '毫秒',
      noDevicesSelected: '暂无关联设备',
      selectDevices: '选择设备',
      selectedDevices: '已选 {count} 个设备'
    },
    monitor: {
      // 实时监控页面
      realtime: {
        title: '实时监控',
        filterStatus: '状态筛选',
        statusAll: '全部',
        statusOnline: '在线',
        statusOffline: '离线',
        pause: '暂停',
        resume: '恢复',
        refresh: '刷新',
        autoRefresh: '自动刷新',
        autoRefreshOff: '关闭',
        deviceCount: '设备数量',
        noDevicesFound: '未找到设备'
      },
      // 设备卡片
      deviceCard: {
        statusOnline: '在线',
        statusOffline: '离线',
        statusError: '错误',
        statusUnknown: '未知',
        deviceId: '设备标识',
        belongTo: '归属',
        updateTime: '更新时间',
        keyIndicators: '关键指标',
        noData: '暂无采集数据'
      },
      // 设备详情对话框
      deviceDetail: {
        title: '设备详情',
        deviceName: '设备名称',
        deviceId: '设备ID',
        status: '状态',
        deviceType: '设备类型',
        protocol: '协议类型',
        location: '位置',
        lastConnect: '最后连接',
        tagValues: '标签值',
        tagName: '标签名称',
        value: '值',
        quality: '质量',
        time: '时间',
        noData: '暂无数据'
      },
      // 历史数据页面
      historical: {
        title: '历史数据',
        timeRange: '时间范围',
        aggregation: '聚合方式',
        refresh: '刷新',
        last15min: '最近15分钟',
        last1hour: '最近1小时',
        last6hours: '最近6小时',
        last1day: '最近1天',
        last7days: '最近7天',
        last30days: '最近30天',
        rawData: '原始数据',
        avg1min: '1分钟平均',
        avg5min: '5分钟平均',
        avg1hour: '1小时平均',
        selectedTags: '已选 {count} 个标签',
        moreTags: '还有 {count} 个',
        selectDeviceHint: '请从左侧树选择设备',
        searchPlaceholder: '搜索设备/标签',
        refreshTree: '刷新'
      },
      // 统计报表页面
      statistics: {
        title: '统计报表',
        byDevice: '按设备',
        byGroup: '按分组',
        byNode: '按节点',
        chart: '图表',
        table: '表格',
        chartView: '图表',
        tableView: '表格',
        refresh: '刷新',
        export: '导出',
        noDataToExport: '没有数据可导出',
        dimension: '维度',
        name: '名称',
        totalPoints: '数据点总数',
        avgDevices: '平均设备数',
        onlineDevices: '在线设备数',
        pointCount: '数据点数',
        to: '至',
        startTime: '开始时间',
        endTime: '结束时间',
        last1Hour: '最近1小时',
        last6Hours: '最近6小时',
        last24Hours: '最近24小时',
        last7Days: '最近7天'
      }
    }
  },
  form: {
    required: '不能为空',
    userName: {
      required: '请输入用户名',
      invalid: '用户名格式不正确'
    },
    phone: {
      required: '请输入手机号',
      invalid: '手机号格式不正确'
    },
    pwd: {
      required: '请输入密码',
      invalid: '至少8位，需包含大小写字母/数字/符号中的至少三类'
    },
    confirmPwd: {
      required: '请输入确认密码',
      invalid: '两次输入密码不一致'
    },
    code: {
      required: '请输入验证码',
      invalid: '验证码格式不正确'
    },
    email: {
      required: '请输入邮箱',
      invalid: '邮箱格式不正确'
    }
  },
  dropdown: {
    closeCurrent: '关闭',
    closeOther: '关闭其它',
    closeLeft: '关闭左侧',
    closeRight: '关闭右侧',
    closeAll: '关闭所有'
  },
  icon: {
    themeConfig: '主题配置',
    themeSchema: '主题模式',
    lang: '切换语言',
    fullscreen: '全屏',
    fullscreenExit: '退出全屏',
    reload: '刷新页面',
    collapse: '折叠菜单',
    expand: '展开菜单',
    pin: '固定',
    unpin: '取消固定'
  },
  datatable: {
    itemCount: '共 {total} 条'
  }
};

export default local;
