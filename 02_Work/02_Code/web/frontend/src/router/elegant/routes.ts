/**
 * 由 Elegant Router 自动生成的路由配置数组。
 *
 * @remarks
 * 每个路由对象都遵循 `GeneratedRoute` 类型，包含路由名称、路径、组件标识、元信息等。
 * 支持嵌套路由（如 manage 下的子路由），可配置权限、菜单显示、图标、排序等属性。
 *
 * @example
 * ```typescript
 * generatedRoutes.forEach(route => {
 *   console.log(route.name, route.path);
 * });
 * ```
 *
 * @see {@link GeneratedRoute}
 */
import type { GeneratedRoute } from '@elegant-router/types';

export const generatedRoutes: GeneratedRoute[] = [
  {
    name: '403',
    path: '/403',
    component: 'layout.blank$view.403',
    meta: {
      title: '403',
      i18nKey: 'route.403',
      constant: true,
      hideInMenu: true
    }
  },
  {
    name: '404',
    path: '/404',
    component: 'layout.blank$view.404',
    meta: {
      title: '404',
      i18nKey: 'route.404',
      constant: true,
      hideInMenu: true
    }
  },
  {
    name: '500',
    path: '/500',
    component: 'layout.blank$view.500',
    meta: {
      title: '500',
      i18nKey: 'route.500',
      constant: true,
      hideInMenu: true
    }
  },
  {
    name: 'alarm',
    path: '/alarm',
    component: 'layout.base',
    meta: {
      title: 'alarm',
      i18nKey: 'route.alarm',
      icon: 'lets-icons:alarm-light',
      roles: ['R_SUPER', 'R_ADMIN', 'R_USER'],
      keepAlive: true,
      order: 5
    },
    children: [
      {
        name: 'alarm_history',
        path: '/alarm/history',
        component: 'view.alarm_history',
        meta: {
          title: 'alarm_history',
          i18nKey: 'route.alarm_history',
          icon: 'material-symbols-light:deployed-code-history-outline-sharp',
          roles: ['R_SUPER', 'R_ADMIN', 'R_USER'],
          keepAlive: true
        }
      },
      {
        name: 'alarm_realtime',
        path: '/alarm/realtime',
        component: 'view.alarm_realtime',
        meta: {
          title: 'alarm_realtime',
          i18nKey: 'route.alarm_realtime',
          icon: 'material-symbols-light:alarm-outline',
          roles: ['R_SUPER', 'R_ADMIN', 'R_USER'],
          keepAlive: true
        }
      },
      {
        name: 'alarm_rule',
        path: '/alarm/rule',
        component: 'view.alarm_rule',
        meta: {
          title: 'alarm_rule',
          i18nKey: 'route.alarm_rule',
          icon: 'bi:file-earmark-ruled',
          roles: ['R_SUPER', 'R_ADMIN', 'R_USER'],
          keepAlive: true
        }
      }
    ]
  },
  {
    name: 'collection',
    path: '/collection',
    component: 'layout.base',
    meta: {
      title: 'collection',
      i18nKey: 'route.collection',
      icon: 'carbon:partition-collection',
      roles: ['R_SUPER', 'R_ADMIN', 'R_USER'],
      keepAlive: true,
      order: 3
    },
    children: [
      {
        name: 'collection_node',
        path: '/collection/node',
        component: 'view.collection_node',
        meta: {
          title: 'collection_node',
          i18nKey: 'route.collection_node',
          icon: 'carbon:kubernetes-worker-node',
          roles: ['R_SUPER', 'R_ADMIN', 'R_USER'],
          keepAlive: true
        }
      },
      {
        name: 'collection_task',
        path: '/collection/task',
        component: 'view.collection_task',
        meta: {
          title: 'collection_task',
          i18nKey: 'route.collection_task',
          icon: 'carbon:task-settings',
          roles: ['R_SUPER', 'R_ADMIN', 'R_USER'],
          keepAlive: true
        }
      }
    ]
  },
  {
    name: 'device',
    path: '/device',
    component: 'layout.base',
    meta: {
      title: 'device',
      i18nKey: 'route.device',
      icon: 'tabler:devices-cog',
      roles: ['R_SUPER', 'R_ADMIN', 'R_USER'],
      keepAlive: true,
      order: 2
    },
    children: [
      {
        name: 'device_label',
        path: '/device/label',
        component: 'view.device_label',
        meta: {
          title: 'device_label',
          i18nKey: 'route.device_label',
          icon: 'carbon:label',
          roles: ['R_SUPER', 'R_ADMIN', 'R_USER'],
          keepAlive: true
        }
      },
      {
        name: 'device_list',
        path: '/device/list',
        component: 'view.device_list',
        meta: {
          title: 'device_list',
          i18nKey: 'route.device_list',
          icon: 'ri:list-settings-line',
          roles: ['R_SUPER', 'R_ADMIN', 'R_USER'],
          keepAlive: true
        }
      },
      {
        name: 'device_protocol',
        path: '/device/protocol',
        component: 'view.device_protocol',
        meta: {
          title: 'device_protocol',
          i18nKey: 'route.device_protocol',
          icon: 'simple-icons:handshake-protocol',
          roles: ['R_SUPER', 'R_ADMIN', 'R_USER'],
          keepAlive: true
        }
      }
    ]
  },
  {
    name: 'home',
    path: '/home',
    component: 'layout.base$view.home',
    meta: {
      title: 'home',
      i18nKey: 'route.home',
      icon: 'mdi:monitor-dashboard',
      roles: ['R_SUPER', 'R_ADMIN', 'R_USER'],
      keepAlive: true,
      order: 1
    }
  },
  {
    name: 'iframe-page',
    path: '/iframe-page/:url',
    component: 'layout.base$view.iframe-page',
    props: true,
    meta: {
      title: 'iframe-page',
      i18nKey: 'route.iframe-page',
      constant: true,
      hideInMenu: true,
      keepAlive: true
    }
  },
  {
    name: 'login',
    path: '/login/:module(pwd-login|code-login|register|reset-pwd|bind-wechat)?',
    component: 'layout.blank$view.login',
    props: true,
    meta: {
      title: 'login',
      i18nKey: 'route.login',
      constant: true,
      hideInMenu: true
    }
  },
  {
    name: 'manage',
    path: '/manage',
    component: 'layout.base',
    meta: {
      title: 'manage',
      i18nKey: 'route.manage',
      icon: 'carbon:cloud-service-management',
      roles: ['R_SUPER', 'R_ADMIN'],
      keepAlive: true,
      order: 6
    },
    children: [
      {
        name: 'manage_menu',
        path: '/manage/menu',
        component: 'view.manage_menu',
        meta: {
          title: 'manage_menu',
          i18nKey: 'route.manage_menu',
          icon: 'material-symbols:route',
          roles: ['R_SUPER'],
          keepAlive: true
        }
      },
      {
        name: 'manage_role',
        path: '/manage/role',
        component: 'view.manage_role',
        meta: {
          title: 'manage_role',
          i18nKey: 'route.manage_role',
          icon: 'carbon:user-role',
          roles: ['R_SUPER']
        }
      },
      {
        name: 'manage_user',
        path: '/manage/user',
        component: 'view.manage_user',
        meta: {
          title: 'manage_user',
          i18nKey: 'route.manage_user',
          icon: 'ic:round-manage-accounts',
          roles: ['R_SUPER', 'R_ADMIN']
        }
      }
    ]
  },
  {
    name: 'monitor',
    path: '/monitor',
    component: 'layout.base',
    meta: {
      title: 'monitor',
      i18nKey: 'route.monitor',
      icon: 'carbon:cloud-monitoring',
      roles: ['R_SUPER', 'R_ADMIN', 'R_USER'],
      keepAlive: true,
      order: 4
    },
    children: [
      {
        name: 'monitor_historical',
        path: '/monitor/historical',
        component: 'view.monitor_historical',
        meta: {
          title: 'monitor_historical',
          i18nKey: 'route.monitor_historical',
          icon: 'iconoir:database-monitor',
          roles: ['R_SUPER', 'R_ADMIN', 'R_USER'],
          keepAlive: true
        }
      },
      {
        name: 'monitor_realtime',
        path: '/monitor/realtime',
        component: 'view.monitor_realtime',
        meta: {
          title: 'monitor_realtime',
          i18nKey: 'route.monitor_realtime',
          icon: 'solar:monitor-camera-broken',
          roles: ['R_SUPER', 'R_ADMIN', 'R_USER'],
          keepAlive: true
        }
      },
      {
        name: 'monitor_statistics',
        path: '/monitor/statistics',
        component: 'view.monitor_statistics',
        meta: {
          title: 'monitor_statistics',
          i18nKey: 'route.monitor_statistics',
          icon: 'mdi:chart-box-outline',
          roles: ['R_SUPER', 'R_ADMIN', 'R_USER'],
          keepAlive: true
        }
      }
    ]
  }
];
