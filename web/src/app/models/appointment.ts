// Shape of an appointment returned by GET /api/appointments (matches the API's AppointmentResponse).
export interface Appointment {
  id: string;
  companyId: string;
  conversationId: string;
  serviceName: string;
  customerName: string;
  scheduledAt: string;
  status: string;
  createdAt: string;
}
