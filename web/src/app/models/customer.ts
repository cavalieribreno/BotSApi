export interface Customer {
  id: string;
  name: string;
  phone: string;
  createdAt: string;
}

export interface CreateCustomerRequest {
  name: string;
  phone: string;
}
