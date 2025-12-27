/** The global namespace for the app */
declare namespace App {
  /** Theme namespace */
  namespace Theme {
    type ColorPaletteNumber = import('@sa/color').ColorPaletteNumber;

    /** Theme setting */
    interface ThemeSetting {
      /** Theme scheme */
      themeScheme: UnionKey.ThemeScheme;
      /** grayscale mode */
      grayscale: boolean;
      /** colour weakness mode */
      colourWeakness: boolean;
      /** Whether to recommend color */
      recommendColor: boolean;
      /** Theme color */
      themeColor: string;
      /** Other color */
      otherColor: OtherColor;
      /** Whether info color is followed by the primary color */
      isInfoFollowPrimary: boolean;
      /** Reset cache strategy */
      resetCacheStrategy: UnionKey.ResetCacheStrategy;
      /** Layout */
      layout: {
        /** Layout mode */
        mode: UnionKey.ThemeLayoutMode;
        /** Scroll mode */
        scrollMode: UnionKey.ThemeScrollMode;
        /**
         * Whether to reverse the horizontal mix
         *
         * if true, the vertical child level menus in left and horizontal first level menus in top
         */
        reverseHorizontalMix: boolean;
      };
      /** Page */
      page: {
        /** Whether to show the page transition */
        animate: boolean;
        /** Page animate mode */
        animateMode: UnionKey.ThemePageAnimateMode;
      };
      /** Header */
      header: {
        /** Header height */
        height: number;
        /** Header breadcrumb */
        breadcrumb: {
          /** Whether to show the breadcrumb */
          visible: boolean;
          /** Whether to show the breadcrumb icon */
          showIcon: boolean;
        };
        /** Multilingual */
        multilingual: {
          /** Whether to show the multilingual */
          visible: boolean;
        };
        /** Global search */
        globalSearch: {
          /** Whether to show the global search */
          visible: boolean;
        };
      };
      /** Tab */
      tab: {
        /** Whether to show the tab */
        visible: boolean;
        /**
         * Whether to cache the tab
         *
         * If cache, the tabs will get from the local storage when the page is refreshed
         */
        cache: boolean;
        /** Tab height */
        height: number;
        /** Tab mode */
        mode: UnionKey.ThemeTabMode;
      };
      /** Fixed header and tab */
      fixedHeaderAndTab: boolean;
      /** Sider */
      sider: {
        /** Inverted sider */
        inverted: boolean;
        /** Sider width */
        width: number;
        /** Collapsed sider width */
        collapsedWidth: number;
        /** Sider width when the layout is 'vertical-mix' or 'horizontal-mix' */
        mixWidth: number;
        /** Collapsed sider width when the layout is 'vertical-mix' or 'horizontal-mix' */
        mixCollapsedWidth: number;
        /** Child menu width when the layout is 'vertical-mix' or 'horizontal-mix' */
        mixChildMenuWidth: number;
      };
      /** Footer */
      footer: {
        /** Whether to show the footer */
        visible: boolean;
        /** Whether fixed the footer */
        fixed: boolean;
        /** Footer height */
        height: number;
        /** Whether float the footer to the right when the layout is 'horizontal-mix' */
        right: boolean;
      };
      /** Watermark */
      watermark: {
        /** Whether to show the watermark */
        visible: boolean;
        /** Watermark text */
        text: string;
        /** Whether to use user name as watermark text */
        enableUserName: boolean;
      };
      /** define some theme settings tokens, will transform to css variables */
      tokens: {
        light: ThemeSettingToken;
        dark?: {
          [K in keyof ThemeSettingToken]?: Partial<ThemeSettingToken[K]>;
        };
      };
    }

    interface OtherColor {
      info: string;
      success: string;
      warning: string;
      error: string;
    }

    interface ThemeColor extends OtherColor {
      primary: string;
    }

    type ThemeColorKey = keyof ThemeColor;

    type ThemePaletteColor = {
      [key in ThemeColorKey | `${ThemeColorKey}-${ColorPaletteNumber}`]: string;
    };

    type BaseToken = Record<string, Record<string, string>>;

    interface ThemeSettingTokenColor {
      /** the progress bar color, if not set, will use the primary color */
      nprogress?: string;
      container: string;
      layout: string;
      inverted: string;
      'base-text': string;
    }

    interface ThemeSettingTokenBoxShadow {
      header: string;
      sider: string;
      tab: string;
    }

    interface ThemeSettingToken {
      colors: ThemeSettingTokenColor;
      boxShadow: ThemeSettingTokenBoxShadow;
    }

    type ThemeTokenColor = ThemePaletteColor & ThemeSettingTokenColor;

    /** Theme token CSS variables */
    type ThemeTokenCSSVars = {
      colors: ThemeTokenColor & { [key: string]: string };
      boxShadow: ThemeSettingTokenBoxShadow & { [key: string]: string };
    };
  }

  /** Global namespace */
  namespace Global {
    type VNode = import('vue').VNode;
    type RouteLocationNormalizedLoaded = import('vue-router').RouteLocationNormalizedLoaded;
    type RouteKey = import('@elegant-router/types').RouteKey;
    type RouteMap = import('@elegant-router/types').RouteMap;
    type RoutePath = import('@elegant-router/types').RoutePath;
    type LastLevelRouteKey = import('@elegant-router/types').LastLevelRouteKey;

    /** The global header props */
    interface HeaderProps {
      /** Whether to show the logo */
      showLogo?: boolean;
      /** Whether to show the menu toggler */
      showMenuToggler?: boolean;
      /** Whether to show the menu */
      showMenu?: boolean;
    }

    /** The global menu */
    type Menu = {
      /**
       * The menu key
       *
       * Equal to the route key
       */
      key: string;
      /** The menu label */
      label: string;
      /** The menu i18n key */
      i18nKey?: I18n.I18nKey | null;
      /** The route key */
      routeKey: RouteKey;
      /** The route path */
      routePath: RoutePath;
      /** The menu icon */
      icon?: () => VNode;
      /** The menu children */
      children?: Menu[];
    };

    type Breadcrumb = Omit<Menu, 'children'> & {
      options?: Breadcrumb[];
    };

    /** Tab route */
    type TabRoute = Pick<RouteLocationNormalizedLoaded, 'name' | 'path' | 'meta'> &
      Partial<Pick<RouteLocationNormalizedLoaded, 'fullPath' | 'query' | 'matched'>>;

    /** The global tab */
    type Tab = {
      /** The tab id */
      id: string;
      /** The tab label */
      label: string;
      /**
       * The new tab label
       *
       * If set, the tab label will be replaced by this value
       */
      newLabel?: string;
      /**
       * The old tab label
       *
       * when reset the tab label, the tab label will be replaced by this value
       */
      oldLabel?: string;
      /** The tab route key */
      routeKey: LastLevelRouteKey;
      /** The tab route path */
      routePath: RouteMap[LastLevelRouteKey];
      /** The tab route full path */
      fullPath: string;
      /** The tab fixed index */
      fixedIndex?: number | null;
      /**
       * Tab icon
       *
       * Iconify icon
       */
      icon?: string;
      /**
       * Tab local icon
       *
       * Local icon
       */
      localIcon?: string;
      /** I18n key */
      i18nKey?: I18n.I18nKey | null;
    };

    /** Form rule */
    type FormRule = import('element-plus').FormItemRule;

    /** The global dropdown key */
    type DropdownKey = 'closeCurrent' | 'closeOther' | 'closeLeft' | 'closeRight' | 'closeAll';
  }

  /**
   * I18n namespace
   *
   * Locales type
   */
  namespace I18n {
    type RouteKey = import('@elegant-router/types').RouteKey;

    type LangType = 'en-US' | 'zh-CN';

    type LangOption = {
      label: string;
      key: LangType;
    };

    type I18nRouteKey = Exclude<RouteKey, 'root' | 'not-found'>;

    type FormMsg = {
      required: string;
      invalid: string;
    };

    type Schema = {
      system: {
        title: string;
        updateTitle: string;
        updateContent: string;
        updateConfirm: string;
        updateCancel: string;
      };
      common: {
        action: string;
        add: string;
        addSuccess: string;
        backToHome: string;
        batchDelete: string;
        cancel: string;
        close: string;
        check: string;
        expandColumn: string;
        columnSetting: string;
        config: string;
        confirm: string;
        createdAt: string;
        delete: string;
        deleteSuccess: string;
        confirmDelete: string;
        edit: string;
        warning: string;
        error: string;
        index: string;
        keywordSearch: string;
        logout: string;
        logoutConfirm: string;
        lookForward: string;
        modify: string;
        modifySuccess: string;
        noData: string;
        operate: string;
        pleaseCheckValue: string;
        refresh: string;
        reset: string;
        search: string;
        switch: string;
        tip: string;
        trigger: string;
        update: string;
        updateSuccess: string;
        userCenter: string;
        status: string;
        import: string;
        export: string;
        pleaseSelect: string;
        pleaseSelectData: string;
        enableSuccess: string;
        disableSuccess: string;
        enable: string;
        disable: string;
        yesOrNo: {
          yes: string;
          no: string;
        };
      };
      request: {
        logout: string;
        logoutMsg: string;
        logoutWithModal: string;
        logoutWithModalMsg: string;
        refreshToken: string;
        tokenExpired: string;
        forbidden: string;
      };
      theme: {
        themeSchema: { title: string } & Record<UnionKey.ThemeScheme, string>;
        grayscale: string;
        colourWeakness: string;
        layoutMode: { title: string; reverseHorizontalMix: string } & Record<UnionKey.ThemeLayoutMode, string>;
        recommendColor: string;
        recommendColorDesc: string;
        themeColor: {
          title: string;
          followPrimary: string;
        } & Theme.ThemeColor;
        scrollMode: { title: string } & Record<UnionKey.ThemeScrollMode, string>;
        page: {
          animate: string;
          mode: { title: string } & Record<UnionKey.ThemePageAnimateMode, string>;
        };
        fixedHeaderAndTab: string;
        header: {
          height: string;
          breadcrumb: {
            visible: string;
            showIcon: string;
          };
          multilingual: {
            visible: string;
          };
          globalSearch: {
            visible: string;
          };
        };
        tab: {
          visible: string;
          cache: string;
          height: string;
          mode: { title: string } & Record<UnionKey.ThemeTabMode, string>;
        };
        sider: {
          inverted: string;
          width: string;
          collapsedWidth: string;
          mixWidth: string;
          mixCollapsedWidth: string;
          mixChildMenuWidth: string;
        };
        footer: {
          visible: string;
          fixed: string;
          height: string;
          right: string;
        };
        watermark: {
          visible: string;
          text: string;
          enableUserName: string;
        };
        themeDrawerTitle: string;
        pageFunTitle: string;
        resetCacheStrategy: { title: string } & Record<UnionKey.ResetCacheStrategy, string>;
        configOperation: {
          copyConfig: string;
          copySuccessMsg: string;
          resetConfig: string;
          resetSuccessMsg: string;
        };
      };
      route: Record<I18nRouteKey, string>;
      page: {
        login: {
          common: {
            loginOrRegister: string;
            userNamePlaceholder: string;
            phonePlaceholder: string;
            codePlaceholder: string;
            passwordPlaceholder: string;
            confirmPasswordPlaceholder: string;
            codeLogin: string;
            confirm: string;
            back: string;
            validateSuccess: string;
            loginSuccess: string;
            welcomeBack: string;
          };
          pwdLogin: {
            title: string;
            rememberMe: string;
            forgetPassword: string;
            register: string;
            otherAccountLogin: string;
            otherLoginMode: string;
            superAdmin: string;
            admin: string;
            user: string;
          };
          codeLogin: {
            title: string;
            getCode: string;
            reGetCode: string;
            sendCodeSuccess: string;
            imageCodePlaceholder: string;
          };
          register: {
            title: string;
            agreement: string;
            protocol: string;
            policy: string;
          };
          resetPwd: {
            title: string;
          };
          bindWeChat: {
            title: string;
          };
        };
        about: {
          title: string;
          introduction: string;
          projectInfo: {
            title: string;
            version: string;
            latestBuildTime: string;
            githubLink: string;
            previewLink: string;
          };
          prdDep: string;
          devDep: string;
        };
        home: {
          greeting: string;
        };
        function: {
          tab: {
            tabOperate: {
              title: string;
              addTab: string;
              addTabDesc: string;
              closeTab: string;
              closeCurrentTab: string;
              closeAboutTab: string;
              addMultiTab: string;
              addMultiTabDesc1: string;
              addMultiTabDesc2: string;
            };
            tabTitle: {
              title: string;
              changeTitle: string;
              change: string;
              resetTitle: string;
              reset: string;
            };
          };
          multiTab: {
            routeParam: string;
            backTab: string;
          };
          toggleAuth: {
            toggleAccount: string;
            authHook: string;
            superAdminVisible: string;
            adminVisible: string;
            adminOrUserVisible: string;
          };
          request: {
            repeatedErrorOccurOnce: string;
            repeatedError: string;
            repeatedErrorMsg1: string;
            repeatedErrorMsg2: string;
          };
        };
        alova: {
          scenes: {
            captchaSend: string;
            autoRequest: string;
            visibilityRequestTips: string;
            pollingRequestTips: string;
            networkRequestTips: string;
            refreshTime: string;
            startRequest: string;
            stopRequest: string;
            requestCrossComponent: string;
            triggerAllRequest: string;
          };
        };
        manage: {
          common: {
            status: {
              enable: string;
              disable: string;
            };
          };
          role: {
            title: string;
            roleName: string;
            roleCode: string;
            roleStatus: string;
            roleDesc: string;
            form: {
              roleName: string;
              roleCode: string;
              roleStatus: string;
              roleDesc: string;
            };
            addRole: string;
            editRole: string;
            menuAuth: string;
            buttonAuth: string;
          };
          user: {
            title: string;
            userName: string;
            userGender: string;
            nickName: string;
            userPhone: string;
            userEmail: string;
            userStatus: string;
            userRole: string;
            form: {
              userName: string;
              userGender: string;
              nickName: string;
              userPhone: string;
              userEmail: string;
              userStatus: string;
              userRole: string;
            };
            addUser: string;
            editUser: string;
            gender: {
              male: string;
              female: string;
            };
          };
          menu: {
            home: string;
            title: string;
            id: string;
            parentId: string;
            menuType: string;
            menuName: string;
            routeName: string;
            routePath: string;
            pathParam: string;
            layout: string;
            page: string;
            i18nKey: string;
            icon: string;
            localIcon: string;
            iconTypeTitle: string;
            order: string;
            constant: string;
            keepAlive: string;
            href: string;
            hideInMenu: string;
            activeMenu: string;
            multiTab: string;
            fixedIndexInTab: string;
            query: string;
            button: string;
            buttonCode: string;
            buttonDesc: string;
            menuStatus: string;
            form: {
              home: string;
              menuType: string;
              menuName: string;
              routeName: string;
              routePath: string;
              pathParam: string;
              layout: string;
              page: string;
              i18nKey: string;
              icon: string;
              localIcon: string;
              order: string;
              keepAlive: string;
              href: string;
              hideInMenu: string;
              activeMenu: string;
              multiTab: string;
              fixedInTab: string;
              fixedIndexInTab: string;
              queryKey: string;
              queryValue: string;
              button: string;
              buttonCode: string;
              buttonDesc: string;
              menuStatus: string;
            };
            addMenu: string;
            editMenu: string;
            addChildMenu: string;
            type: {
              directory: string;
              menu: string;
            };
            iconType: {
              iconify: string;
              local: string;
            };
          };
        };
        device: {
          title: string;
          deviceName: string;
          deviceId: string;
          description: string;
          protocol: string;
          protocolTypeLabel: string;
          connection: string;
          edgeNode: string;
          deviceGroup: string;
          location: string;
          enabled: string;
          tagCount: string;
          lastConnectedAt: string;
          connectionConfig: string;
          protocolConfig: string;
          tagsConfig: string;
          testConnection: string;
          form: {
            deviceName: string;
            deviceId: string;
            description: string;
            protocol: string;
            edgeNode: string;
            deviceGroup: string;
            location: string;
            enabled: string;
          };
          addDevice: string;
          editDevice: string;
          protocolType: {
            modbusTcp: string;
            modbusRtu: string;
            opcUa: string;
            opcDa: string;
            s7: string;
            bacnet: string;
            other: string;
          };
          connectionStatus: {
            connected: string;
            disconnected: string;
            error: string;
            unknown: string;
          };
          noEdgeNodeFound: string;
          noDeviceGroupFound: string;
          pleaseSelectDevicesToDelete: string;
          batchDeleteSuccess: string;
          deviceDisabled: string;
          deviceEnabled: string;
          missingDeviceId: string;
          disable: string;
          enable: string;
          connectionForm: {
            ip: string;
            ipPlaceholder: string;
            port: string;
            timeout: string;
            retryCount: string;
            enableEncryption: string;
          };
          protocolForm: {
            // 通用字段
            ip: string;
            ipPlaceholder: string;
            port: string;
            // Modbus TCP/RTU
            unitId: string;
            pollingInterval: string;
            serialPort: string;
            serialPortPlaceholder: string;
            baudRate: string;
            dataBits: string;
            stopBits: string;
            parity: string;
            slaveId: string;
            frameInterval: string;
            // OPC UA
            serverUrl: string;
            serverUrlPlaceholder: string;
            securityMode: string;
            securityModePlaceholder: string;
            securityPolicy: string;
            securityPolicyPlaceholder: string;
            authenticationMode: string;
            authenticationModePlaceholder: string;
            samplingInterval: string;
            // OPC DA
            serverName: string;
            serverNamePlaceholder: string;
            clsid: string;
            clsidPlaceholder: string;
            updateRate: string;
            // S7
            cpuType: string;
            rack: string;
            slot: string;
          };
        };
        deviceGroup: {
          title: string;
          tree: string;
          name: string;
          description: string;
          level: string;
          sortOrder: string;
          deviceCount: string;
          childCount: string;
          parent: string;
          addChild: string;
          showAll: string;
          searchPlaceholder: string;
          namePlaceholder: string;
          parentPlaceholder: string;
          descriptionPlaceholder: string;
          nameRequired: string;
          nameLength: string;
          maxLevelReached: string;
          levelWarning: string;
        };
        tag: {
          // 页面标题和列表
          deviceList: string;
          tagList: string;
          searchDevice: string;
          noDevice: string;
          tagCount: string;
          selectDeviceFirst: string;
          showEnabledOnly: string;

          // 标签字段
          tagId: string;
          tagName: string;
          tagAddress: string;
          dataTypeLabel: string;
          unit: string;
          description: string;
          accessModeLabel: string;
          minValue: string;
          maxValue: string;
          scalingFactor: string;
          offset: string;
          deadband: string;

          // 占位符
          tagIdPlaceholder: string;
          tagNamePlaceholder: string;
          tagAddressPlaceholder: string;
          dataTypePlaceholder: string;
          statusPlaceholder: string;
          unitPlaceholder: string;
          descriptionPlaceholder: string;

          // 验证消息
          tagIdRequired: string;
          tagNameRequired: string;
          dataTypeRequired: string;

          // 操作
          addTag: string;
          editTag: string;
          batchEnable: string;
          batchDisable: string;
          confirmBatchDelete: string;
          exportSuccess: string;
          importNotImplemented: string;

          // 区域标题
          basicInfo: string;
          addressConfig: string;
          advancedConfig: string;

          // 数据类型
          dataType: {
            int16: string;
            int32: string;
            int64: string;
            uint16: string;
            uint32: string;
            uint64: string;
            float: string;
            double: string;
            boolean: string;
            string: string;
          };

          // 访问模式
          accessMode: {
            readOnly: string;
            writeOnly: string;
            readWrite: string;
          };

          // Modbus 地址配置
          modbus: {
            functionCode: string;
            address: string;
            slaveId: string;
            quantity: string;
          };

          // OPC UA 地址配置
          opcua: {
            nodeId: string;
            nodeIdPlaceholder: string;
            namespaceIndex: string;
          };

          // OPC DA 地址配置
          opcda: {
            itemId: string;
            itemIdPlaceholder: string;
          };

          // S7 地址配置
          s7: {
            area: string;
            dbNumber: string;
            offset: string;
            bitOffset: string;
          };
        };
        edgeNode: {
          title: string;
          nodeName: string;
          nodeId: string;
          platform: string;
          version: string;
          status: string;
          registrationType: string;
          ipAddress: string;
          port: string;
          location: string;
          deviceCount: string;
          lastHeartbeat: string;
          osInfo: string;
          hardwareInfo: string;
          installPath: string;
          resourceLimits: string;
          basicInfo: string;
          systemInfo: string;
          advancedConfig: string;
          form: {
            nodeName: string;
            nodeId: string;
            status: string;
            platform: string;
            location: string;
            resourceLimits: string;
            version: string;
            ipAddress: string;
            installPath: string;
            osInfo: string;
            hardwareInfo: string;
          };
          addNode: string;
          editNode: string;
          nodeStatus: {
            online: string;
            offline: string;
            error: string;
          };
          platformType: {
            net80: string;
            net45: string;
          };
          registrationTypeOptions: {
            auto: string;
            manual: string;
          };
          confirmDeleteNode: string;
          deleteNodeWithDevicesWarning: string;
          deleteSuccess: string;
          confirmBatchDelete: string;
          batchDeleteSuccess: string;
          batchDeletePartialSuccess: string;
          editableFieldsNote: string;
          manualNodeNote: string;
          manualNodeEditableNote: string;
          manualNodeConnectedNote: string;
          autoNodeEditNote: string;
        };
        collectionTask: {
          title: string;
          name: string;
          code: string;
          description: string;
          taskType: string;
          defaultInterval: string;
          cronExpression: string;
          priority: string;
          status: string;
          isEnabled: string;
          effectiveFrom: string;
          effectiveTo: string;
          deviceCount: string;
          devices: string;
          form: {
            name: string;
            code: string;
            description: string;
            taskType: string;
            defaultInterval: string;
            cronExpression: string;
            priority: string;
            devices: string;
          };
          addTask: string;
          editTask: string;
          taskTypeOptions: {
            periodic: string;
            scheduled: string;
            eventDriven: string;
            hybrid: string;
          };
          taskStatusOptions: {
            draft: string;
            active: string;
            paused: string;
            stopped: string;
          };
          taskTypeDescription: {
            periodic: string;
            scheduled: string;
            eventDriven: string;
            hybrid: string;
          };
          confirmDeleteTask: string;
          deleteSuccess: string;
          confirmBatchDelete: string;
          batchDeleteSuccess: string;
          batchDeletePartialSuccess: string;
          statusChangeSuccess: string;
          startTask: string;
          pauseTask: string;
          stopTask: string;
          confirmStartTask: string;
          confirmPauseTask: string;
          confirmStopTask: string;
          confirmEnableTask: string;
          confirmDisableTask: string;
          enableTask: string;
          disableTask: string;
          cronExpressionHelp: string;
          intervalMs: string;
          noDevicesSelected: string;
          selectDevices: string;
          selectedDevices: string;
        };
      };
      form: {
        required: string;
        userName: FormMsg;
        phone: FormMsg;
        pwd: FormMsg;
        confirmPwd: FormMsg;
        code: FormMsg;
        email: FormMsg;
      };
      dropdown: Record<Global.DropdownKey, string>;
      icon: {
        themeConfig: string;
        themeSchema: string;
        lang: string;
        fullscreen: string;
        fullscreenExit: string;
        reload: string;
        collapse: string;
        expand: string;
        pin: string;
        unpin: string;
      };
      datatable: {
        itemCount: string;
      };
    };

    type GetI18nKey<T extends Record<string, unknown>, K extends keyof T = keyof T> = K extends string
      ? T[K] extends Record<string, unknown>
        ? `${K}.${GetI18nKey<T[K]>}`
        : K
      : never;

    type I18nKey = GetI18nKey<Schema>;

    type TranslateOptions<Locales extends string> = import('vue-i18n').TranslateOptions<Locales>;

    interface $T {
      (key: I18nKey): string;
      (key: I18nKey, plural: number, options?: TranslateOptions<LangType>): string;
      (key: I18nKey, defaultMsg: string, options?: TranslateOptions<I18nKey>): string;
      (key: I18nKey, list: unknown[], options?: TranslateOptions<I18nKey>): string;
      (key: I18nKey, list: unknown[], plural: number): string;
      (key: I18nKey, list: unknown[], defaultMsg: string): string;
      (key: I18nKey, named: Record<string, unknown>, options?: TranslateOptions<LangType>): string;
      (key: I18nKey, named: Record<string, unknown>, plural: number): string;
      (key: I18nKey, named: Record<string, unknown>, defaultMsg: string): string;
    }
  }

  /** Service namespace */
  namespace Service {
    /** Other baseURL key */
    type OtherBaseURLKey = 'demo';

    interface ServiceConfigItem {
      /** The backend service base url */
      baseURL: string;
      /** The proxy pattern of the backend service base url */
      proxyPattern: string;
    }

    interface OtherServiceConfigItem extends ServiceConfigItem {
      key: OtherBaseURLKey;
    }

    /** The backend service config */
    interface ServiceConfig extends ServiceConfigItem {
      /** Other backend service config */
      other: OtherServiceConfigItem[];
    }

    interface SimpleServiceConfig extends Pick<ServiceConfigItem, 'baseURL'> {
      other: Record<OtherBaseURLKey, string>;
    }

    /** The backend service response data */
    type Response<T = unknown> = {
      /** The backend service response code */
      code: string;
      /** The backend service response message */
      msg: string;
      /** The backend service response data */
      data: T;
    };

    /** The demo backend service response data */
    type DemoResponse<T = unknown> = {
      /** The backend service response code */
      status: string;
      /** The backend service response message */
      message: string;
      /** The backend service response data */
      result: T;
    };
  }
}
