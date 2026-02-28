declare namespace Api {
  /**
   * namespace SystemManage
   *
   * backend api module: "systemManage"
   */
  namespace SystemManage {
    type CommonSearchParams = Pick<Common.PaginatingCommonParams, 'current' | 'size'>;

    /** role */
    type Role = {
      /** role id */
      id: string;
      /** role name */
      roleName: string;
      /** role code */
      roleCode: string;
      /** role description */
      roleDesc: string;
      /** role status */
      status: Common.EnableStatus | undefined;
      /** created by user id - audit field */
      createdBy?: string;
      /** updated by user id - audit field */
      updatedBy?: string;
      /** created at timestamp - audit field */
      createdAt?: string;
      /** updated at timestamp - audit field */
      updatedAt?: string;
      /** deleted flag - audit field */
      deletedFlag?: boolean;
      /** tenant id - audit field */
      tenantId?: string;
    };

    /** role edit */
    type RoleEdit = {
      /** role name */
      roleName: string;
      /** role code */
      roleCode: string;
      /** role description */
      roleDesc?: string;
      /** role status - number for API, string for display */
      status?: number;
    };

    /** role search params */
    type RoleSearchParams = CommonType.RecordNullable<
      Pick<Api.SystemManage.Role, 'roleName' | 'roleCode' | 'status'> & CommonSearchParams
    >;

    /** role list */
    type RoleList = Common.PaginatingQueryRecord<Role>;

    /** all role */
    type AllRole = Pick<Role, 'id' | 'roleName' | 'roleCode'>;

    /**
     * user gender
     *
     * - "1": "male"
     * - "2": "female"
     */
    type UserGender = '1' | '2';

    /**
     * user type
     *
     * - "user": normal user account (人员账号)
     * - "service": service account (服务账号)
     */
    type UserType = 'user' | 'service';

    /** user */
    type User = {
      /** user id */
      id: string;
      /** user name */
      userName: string;
      /** user gender */
      userGender: UserGender | undefined;
      /** user nick name */
      nickName: string;
      /** user phone */
      userPhone: string;
      /** user email */
      userEmail: string;
      /** user status */
      status: Common.EnableStatus | undefined;
      /** user type - user or service */
      userType?: UserType;
      /** user role code collection */
      userRoles: string[];
      /** created by user id - audit field */
      createdBy?: string;
      /** updated by user id - audit field */
      updatedBy?: string;
      /** created at timestamp - audit field */
      createdAt?: string;
      /** updated at timestamp - audit field */
      updatedAt?: string;
      /** deleted flag - audit field */
      deletedFlag?: boolean;
      /** tenant id - audit field */
      tenantId?: string;
    };

    /** user edit */
    type UserEdit = {
      /** user name */
      userName: string;
      /** user nick name */
      nickName?: string;
      /** user gender - number for API */
      userGender?: number;
      /** user phone */
      userPhone?: string;
      /** user email */
      userEmail?: string;
      /** user status - number for API */
      status?: number;
      /** user type - user or service */
      userType?: UserType;
      /** user role code collection */
      userRoles?: string[];
      /** user password - optional, only for add/edit */
      password?: string;
    };

    /** user search params */
    type UserSearchParams = CommonType.RecordNullable<
      Pick<
        Api.SystemManage.User,
        'userName' | 'userGender' | 'nickName' | 'userPhone' | 'userEmail' | 'status' | 'userType'
      > &
        CommonSearchParams
    >;

    /** user list */
    type UserList = Common.PaginatingQueryRecord<User>;

    /**
     * menu type
     *
     * - "1": directory
     * - "2": menu
     */
    type MenuType = '1' | '2';

    type MenuButton = {
      /**
       * button code
       *
       * it can be used to control the button permission
       */
      code: string;
      /** button description */
      desc: string;
    };

    /**
     * icon type
     *
     * - "1": iconify icon
     * - "2": local icon
     */
    type IconType = '1' | '2';

    type MenuPropsOfRoute = Pick<
      import('vue-router').RouteMeta,
      | 'i18nKey'
      | 'keepAlive'
      | 'constant'
      | 'order'
      | 'href'
      | 'hideInMenu'
      | 'activeMenu'
      | 'multiTab'
      | 'fixedIndexInTab'
      | 'query'
    >;

    type Menu = Common.CommonRecord<{
      /** parent menu id */
      parentId: number;
      /** menu type */
      menuType: MenuType;
      /** menu name */
      menuName: string;
      /** route name */
      routeName: string;
      /** route path */
      routePath: string;
      /** component */
      component?: string;
      /** iconify icon name or local icon name */
      icon: string;
      /** icon type */
      iconType: IconType;
      /** buttons */
      buttons?: MenuButton[] | null;
      /** children menu */
      children?: Menu[] | null;
    }> &
      MenuPropsOfRoute;

    /** menu list */
    type MenuList = Common.PaginatingQueryRecord<Menu>;

    type MenuTree = {
      id: number;
      label: string;
      pId: number;
      children?: MenuTree[];
    };
  }
}
