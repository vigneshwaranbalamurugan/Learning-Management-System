export interface LoginModel {
  email: string;
  password?: string;
}

export interface RegisterModel {
  email: string;
  password?: string;
  confirmPassword?: string;
  role: string;
}

export interface ForgotPasswordModel {
  email: string;
}

export interface ResendVerificationModel {
  email: string;
}

export interface ResetPasswordModel {
  email: string;
  token: string;
  newPassword: string;
  confirmPassword?: string;
}
