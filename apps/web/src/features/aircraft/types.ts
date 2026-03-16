import type { BaseEntity, AircraftStatus } from '@/types/common';

export interface Aircraft extends BaseEntity {
  registration: string;
  serial_number: string;
  aircraft_type: string;
  manufacturer: string;
  manufacture_date: string | null;
  status: AircraftStatus;
  remarks: string | null;
}

export interface AircraftCounter {
  aircraft_id: string;
  counter_type: string;
  value: number;
  unit: string;
  recorded_at: string;
}

export interface AircraftListParams {
  status?: AircraftStatus;
  page?: number;
  page_size?: number;
}
