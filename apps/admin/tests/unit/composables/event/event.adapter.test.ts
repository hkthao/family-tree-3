import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ApiEventServiceAdapter } from '@/composables/event/event.adapter';
import type { IEventService } from '@/services/event/event.service.interface';
import { CalendarType, RepeatRule, EventType } from '@/types';
import type { EventDto, Result, ApiError } from '@/types';

// Mock ApiEventService
const mockApiEventService: IEventService = {
  add: vi.fn(),
  update: vi.fn(),
  delete: vi.fn(),
  getById: vi.fn(),
  search: vi.fn(),
  getEventsByFamilyId: vi.fn(),
  getByIds: vi.fn(),
};

describe('ApiEventServiceAdapter', () => {
  let adapter: ApiEventServiceAdapter;

  beforeEach(() => {
    // Reset mocks before each test
    vi.clearAllMocks();
    adapter = new ApiEventServiceAdapter(mockApiEventService);
  });

  const mockEvent: EventDto = {
    id: '1',
    name: 'Test EventDto',
    code: 'TE001',
    type: EventType.Other,
    familyId: 'family1',
    calendarType: CalendarType.Solar,
    solarDate: new Date('2023-01-01'),
    lunarDate: { day: 1, month: 1, isLeapMonth: false },
    repeatRule: RepeatRule.None,
    description: 'A test description',
    color: '#FF0000',
    relatedMemberIds: ['member1'],
  };

  const mockError: ApiError = {
    statusCode: 400,
    message: 'Bad Request',
    details: [],
    name: ''
  };

  // Helper function for successful Result
  const successResult = <T>(value: T): Result<T> => ({ ok: true, value });
  // Helper function for error Result
  const errorResult = <T>(error: ApiError): Result<T> => ({ ok: false, error });

  it('nên gọi apiEventService.add và trả về kết quả', async () => {
    mockApiEventService.add = vi.fn().mockResolvedValue(successResult(mockEvent));

    const eventDataWithoutId: Omit<EventDto, 'id'> = {
      name: 'Test EventDto',
      code: 'TE001',
      type: EventType.Other,
      familyId: 'family1',
      calendarType: CalendarType.Solar,
      solarDate: new Date('2023-01-01'),
      lunarDate: { day: 1, month: 1, isLeapMonth: false },
      repeatRule: RepeatRule.None,
      description: 'A test description',
      color: '#FF0000',
      relatedMemberIds: ['member1'],
    };
    const result = await adapter.add(eventDataWithoutId);

    expect(mockApiEventService.add).toHaveBeenCalledWith(eventDataWithoutId);
    expect(result).toEqual(successResult(mockEvent));
  });

  it('nên xử lý lỗi từ apiEventService.add', async () => {
    mockApiEventService.add = vi.fn().mockResolvedValue(errorResult(mockError));

    const eventDataWithoutId: Omit<EventDto, 'id'> = {
      name: 'Test EventDto',
      code: 'TE001',
      type: EventType.Other,
      familyId: 'family1',
      calendarType: CalendarType.Solar,
      solarDate: new Date('2023-01-01'),
      lunarDate: { day: 1, month: 1, isLeapMonth: false },
      repeatRule: RepeatRule.None,
      description: 'A test description',
      color: '#FF0000',
      relatedMemberIds: ['member1'],
    };
    const result = await adapter.add(eventDataWithoutId);

    expect(mockApiEventService.add).toHaveBeenCalledWith(eventDataWithoutId);
    expect(result).toEqual(errorResult(mockError));
  });

  it('nên gọi apiEventService.update và trả về kết quả', async () => {
    mockApiEventService.update = vi.fn().mockResolvedValue(successResult(mockEvent));

    const result = await adapter.update(mockEvent);

    expect(mockApiEventService.update).toHaveBeenCalledWith(mockEvent);
    expect(result).toEqual(successResult(mockEvent));
  });

  it('nên xử lý lỗi từ apiEventService.update', async () => {
    mockApiEventService.update = vi.fn().mockResolvedValue(errorResult(mockError));

    const result = await adapter.update(mockEvent);

    expect(mockApiEventService.update).toHaveBeenCalledWith(mockEvent);
    expect(result).toEqual(errorResult(mockError));
  });

  it('nên gọi apiEventService.delete và trả về kết quả', async () => {
    mockApiEventService.delete = vi.fn().mockResolvedValue(successResult(undefined));

    const result = await adapter.delete(mockEvent.id!);

    expect(mockApiEventService.delete).toHaveBeenCalledWith(mockEvent.id);
    expect(result).toEqual(successResult(undefined));
  });

  it('nên xử lý lỗi từ apiEventService.delete', async () => {
    mockApiEventService.delete = vi.fn().mockResolvedValue(errorResult(mockError));

    const result = await adapter.delete(mockEvent.id!);

    expect(mockApiEventService.delete).toHaveBeenCalledWith(mockEvent.id);
    expect(result).toEqual(errorResult(mockError));
  });

  it('nên gọi apiEventService.getById và trả về kết quả', async () => {
    mockApiEventService.getById = vi.fn().mockResolvedValue(successResult(mockEvent));

    const result = await adapter.getById(mockEvent.id!);

    expect(mockApiEventService.getById).toHaveBeenCalledWith(mockEvent.id);
    expect(result).toEqual(successResult(mockEvent));
  });

  it('nên xử lý lỗi từ apiEventService.getById', async () => {
    mockApiEventService.getById = vi.fn().mockResolvedValue(errorResult(mockError));

    const result = await adapter.getById(mockEvent.id!);

    expect(mockApiEventService.getById).toHaveBeenCalledWith(mockEvent.id);
    expect(result).toEqual(errorResult(mockError));
  });

  it('nên gọi apiEventService.search và trả về kết quả', async () => {
    const mockPaginationResult = {
      items: [mockEvent],
      totalItems: 1,
      totalPages: 1,
      page: 1,
      itemsPerPage: 10,
    };
    mockApiEventService.search = vi.fn().mockResolvedValue(successResult(mockPaginationResult));

    const listOptions = { page: 1, itemsPerPage: 10 };
    const filterOptions = { familyId: 'family1' };
    const result = await adapter.search(listOptions, filterOptions);

    expect(mockApiEventService.search).toHaveBeenCalledWith(listOptions, filterOptions);
    expect(result).toEqual(successResult(mockPaginationResult));
  });

  it('nên xử lý lỗi từ apiEventService.search', async () => {
    mockApiEventService.search = vi.fn().mockResolvedValue(errorResult(mockError));

    const listOptions = { page: 1, itemsPerPage: 10 };
    const filterOptions = { familyId: 'family1' };
    const result = await adapter.search(listOptions, filterOptions);

    expect(mockApiEventService.search).toHaveBeenCalledWith(listOptions, filterOptions);
    expect(result).toEqual(errorResult(mockError));
  });

  it('nên gọi apiEventService.getEventsByFamilyId và trả về kết quả', async () => {
    mockApiEventService.getEventsByFamilyId = vi.fn().mockResolvedValue(successResult([mockEvent]));

    const result = await adapter.getEventsByFamilyId(mockEvent.familyId!);

    expect(mockApiEventService.getEventsByFamilyId).toHaveBeenCalledWith(mockEvent.familyId);
    expect(result).toEqual(successResult([mockEvent]));
  });

  it('nên xử lý lỗi từ apiEventService.getEventsByFamilyId', async () => {
    mockApiEventService.getEventsByFamilyId = vi.fn().mockResolvedValue(errorResult(mockError));

    const result = await adapter.getEventsByFamilyId(mockEvent.familyId!);

    expect(mockApiEventService.getEventsByFamilyId).toHaveBeenCalledWith(mockEvent.familyId);
    expect(result).toEqual(errorResult(mockError));
  });

  it('nên gọi apiEventService.getByIds và trả về kết quả', async () => {
    mockApiEventService.getByIds = vi.fn().mockResolvedValue(successResult([mockEvent]));

    const result = await adapter.getByIds([mockEvent.id!]);

    expect(mockApiEventService.getByIds).toHaveBeenCalledWith([mockEvent.id]);
    expect(result).toEqual(successResult([mockEvent]));
  });

  it('nên xử lý lỗi từ apiEventService.getByIds', async () => {
    mockApiEventService.getByIds = vi.fn().mockResolvedValue(errorResult(mockError));

    const result = await adapter.getByIds([mockEvent.id!]);

    expect(mockApiEventService.getByIds).toHaveBeenCalledWith([mockEvent.id]);
    expect(result).toEqual(errorResult(mockError));
  });
});
