import { ref, toValue } from 'vue';
import type { ComputedRef, Ref } from 'vue';
import type { FormInstance } from 'element-plus';
import { REG_CODE_SIX, REG_EMAIL, REG_PHONE, REG_PWD, REG_USER_NAME } from '@/constants/reg';
import { $t } from '@/locales';

export function useFormRules() {
  const patternRules = {
    userName: {
      pattern: REG_USER_NAME,
      message: $t('form.userName.invalid'),
      trigger: 'change'
    },
    phone: {
      pattern: REG_PHONE,
      message: $t('form.phone.invalid'),
      trigger: 'change'
    },
    // 登录场景不做强校验，仅保留必填；保留 REG_PWD 供兼容，但默认不引用
    pwd: {
      pattern: REG_PWD,
      message: $t('form.pwd.invalid'),
      trigger: 'change'
    },
    code: {
      pattern: REG_CODE_SIX,
      message: $t('form.code.invalid'),
      trigger: 'change'
    },
    email: {
      pattern: REG_EMAIL,
      message: $t('form.email.invalid'),
      trigger: 'change'
    }
  } satisfies Record<string, App.Global.FormRule>;

  const formRules = {
    userName: [createRequiredRule($t('form.userName.required')), patternRules.userName],
    phone: [createRequiredRule($t('form.phone.required')), patternRules.phone],
    // 登录页仅必填，不做格式限制（避免与后端强策略冲突）
    pwd: [createRequiredRule($t('form.pwd.required'))],
    code: [createRequiredRule($t('form.code.required')), patternRules.code],
    email: [createRequiredRule($t('form.email.required')), patternRules.email]
  } satisfies Record<string, App.Global.FormRule[]>;

  /** the default required rule */
  const defaultRequiredRule = createRequiredRule($t('form.required'));

  function createRequiredRule(message: string): App.Global.FormRule {
    return {
      required: true,
      message
    };
  }

  /** create a rule for confirming the password */
  function createConfirmPwdRule(pwd: string | Ref<string> | ComputedRef<string>) {
    const confirmPwdRule: App.Global.FormRule[] = [
      { required: true, message: $t('form.confirmPwd.required') },
      {
        asyncValidator: (rule, value) => {
          if (value.trim() !== '' && value !== toValue(pwd)) {
            return Promise.reject(rule.message);
          }
          return Promise.resolve();
        },
        message: $t('form.confirmPwd.invalid'),
        trigger: 'input'
      }
    ];
    return confirmPwdRule;
  }

  /** 强密码校验：至少8位，包含大小写/数字/符号中至少三类 */
  function createStrongPwdRule(): App.Global.FormRule[] {
    return [
      createRequiredRule($t('form.pwd.required')),
      {
        asyncValidator: (_rule, value: string) => {
          const pwd = String(value ?? '');
          if (pwd.length < 8) return Promise.reject($t('form.pwd.invalid'));
          let kinds = 0;
          if (/[a-z]/.test(pwd)) kinds += 1;
          if (/[A-Z]/.test(pwd)) kinds += 1;
          if (/[0-9]/.test(pwd)) kinds += 1;
          if (/[^a-zA-Z0-9_\s]/.test(pwd)) kinds += 1; // 符号（不含下划线）
          return kinds >= 3 ? Promise.resolve() : Promise.reject($t('form.pwd.invalid'));
        },
        trigger: 'change'
      }
    ];
  }

  return {
    patternRules,
    formRules,
    defaultRequiredRule,
    createRequiredRule,
    createConfirmPwdRule,
    createStrongPwdRule
  };
}

export function useForm() {
  const formRef = ref<FormInstance | null>(null);

  async function validate() {
    await formRef.value?.validate();
  }

  async function restoreValidation() {
    formRef.value?.resetFields();
  }

  return {
    formRef,
    validate,
    restoreValidation
  };
}
