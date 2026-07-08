export interface CreatePaymentIntentRequest {
  bookingId: number;
  amount: number;
  currency: string;
  paymentMethodType: string;
}

export interface PaymentIntentResponse {
  paymentIntentId: string;
  clientSecret: string;
}

export interface ConfirmPaymentRequest {
  bookingId: number;
  paymentIntentId: string;
}

export interface ConfirmPaymentResponse {
  success: boolean;
  message?: string;
}

export interface RefundRequest {
  bookingId: number;
  paymentIntentId: string;
}

export interface RefundResponse {
  success: boolean;
  message?: string;
}
