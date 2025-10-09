import { computed } from 'vue';
import { useCountDown, useLoading } from '@sa/hooks';
import { REG_PHONE } from '@/constants/reg';
import { $t } from '@/locales';

/**
 * 用于验证码获取和倒计时的业务逻辑 Hook。
 *
 * - 提供验证码按钮的文本状态（如“获取验证码”、“重新获取”等）。
 * - 校验手机号格式和必填项。
 * - 管理验证码请求的加载状态和倒计时逻辑。
 * - 支持验证码发送成功后的提示。
 *
 * @returns {{
 *   label: ComputedRef<string>; // 验证码按钮显示文本
 *   start: () => void;          // 开始倒计时
 *   stop: () => void;           // 停止倒计时
 *   isCounting: Ref<boolean>;   // 是否正在倒计时
 *   loading: Ref<boolean>;      // 请求验证码时的加载状态
 *   getCaptcha: (phone: string) => Promise<void>; // 请求验证码
 * }}
 */
export function useCaptcha() {
  const { loading, startLoading, endLoading } = useLoading();
  const { count, start, stop, isCounting } = useCountDown(10);

  const label = computed(() => {
    let text = $t('page.login.codeLogin.getCode');

    const countingLabel = $t('page.login.codeLogin.reGetCode', { time: count.value });

    if (loading.value) {
      text = '';
    }

    if (isCounting.value) {
      text = countingLabel;
    }

    return text;
  });

  function isPhoneValid(phone: string) {
    if (phone.trim() === '') {
      window.$message?.error?.($t('form.phone.required'));

      return false;
    }

    if (!REG_PHONE.test(phone)) {
      window.$message?.error?.($t('form.phone.invalid'));

      return false;
    }

    return true;
  }

  async function getCaptcha(phone: string) {
    const valid = isPhoneValid(phone);

    if (!valid || loading.value) {
      return;
    }

    startLoading();

    // request
    await new Promise(resolve => {
      setTimeout(resolve, 500);
    });

    window.$message?.success?.($t('page.login.codeLogin.sendCodeSuccess'));

    start();

    endLoading();
  }

  return {
    label,
    start,
    stop,
    isCounting,
    loading,
    getCaptcha
  };
}
