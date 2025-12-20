// src/composables/event/eventCalendar.adapter.ts
import { Solar, Lunar, SolarUtil } from 'lunar-javascript';
import dayjs from 'dayjs';
/**
 * @interface DateAdapter
 * @description Adapts date manipulation functionalities.
 */
export interface DateAdapter {
  startOfMonth(date: Date | string): Date;
  endOfMonth(date: Date | string): Date;
  getFullYear(date: Date): number;
  getMonth(date: Date): number; // 0-indexed
  getDate(date: Date): number;
  newDate(year?: number, month?: number, day?: number): Date;
}

/**
 * @class DayjsDateAdapter
 * @description Implements DateAdapter using dayjs library.
 */
export class DayjsDateAdapter implements DateAdapter {
  startOfMonth(date: Date | string): Date {
    return dayjs(date).startOf('month').toDate();
  }
  endOfMonth(date: Date | string): Date {
    return dayjs(date).endOf('month').toDate();
  }
  getFullYear(date: Date): number {
    return dayjs(date).year();
  }
  getMonth(date: Date): number {
    return dayjs(date).month();
  }
  getDate(date: Date): number {
    return dayjs(date).date();
  }
  newDate(year?: number, month?: number, day?: number): Date {
    if (year !== undefined && month !== undefined && day !== undefined) {
      return dayjs().year(year).month(month).date(day).toDate();
    }
    return dayjs().toDate();
  }
}

/**
 * @interface LunarDateAdapter
 * @description Adapts lunar-javascript library functionalities.
 */
export interface LunarDateAdapter {
  lunarFromYmd(year: number, month: number, day: number): InstanceType<typeof Lunar>;
  solarFromYmd(year: number, month: number, day: number): InstanceType<typeof Solar>;
  fromSolar(solar: InstanceType<typeof Solar>): InstanceType<typeof Lunar>;
  getSolar(lunar: InstanceType<typeof Lunar>): InstanceType<typeof Solar>;
  getLunarDaysInMonth(year: number, month: number): number;
}

export type LunarInstance = InstanceType<typeof Lunar>;
export type SolarInstance = InstanceType<typeof Solar>;


/**
 * @class LunarJsDateAdapter
 * @description Implements LunarDateAdapter using lunar-javascript library.
 */
export class LunarJsDateAdapter implements LunarDateAdapter {
  lunarFromYmd(year: number, month: number, day: number): InstanceType<typeof Lunar> {
    return Lunar.fromYmd(year, month, day);
  }
  solarFromYmd(year: number, month: number, day: number): InstanceType<typeof Solar> {
    return Solar.fromYmd(year, month, day);
  }
  fromSolar(solar: InstanceType<typeof Solar>): InstanceType<typeof Lunar> {
    return Lunar.fromSolar(solar);
  }
  getSolar(lunar: InstanceType<typeof Lunar>): InstanceType<typeof Solar> {
    return lunar.getSolar();
  }
  getLunarDaysInMonth(year: number, month: number): number {
    return SolarUtil.getDaysOfMonth(year, month);
  }
}

/**
 * @constant DefaultDateAdapter
 * @description Provides a default instance of the DayjsDateAdapter.
 */
export const DefaultDateAdapter: DateAdapter = new DayjsDateAdapter();

/**
 * @constant DefaultLunarDateAdapter
 * @description Provides a default instance of the LunarJsDateAdapter.
 */
export const DefaultLunarDateAdapter: LunarDateAdapter = new LunarJsDateAdapter();
