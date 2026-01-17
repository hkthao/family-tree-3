import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { createDefaultF3Adapter, F3Adapter } from '@/composables/charts/f3.adapter';
import f3 from 'family-chart';

// Mock the f3 library
vi.mock('family-chart', async (importOriginal) => {
  const mod = await importOriginal<typeof f3>();
  const mockChartInstance = {
    setTransitionTime: vi.fn().mockReturnThis(),
    setCardXSpacing: vi.fn().mockReturnThis(),
    setCardYSpacing: vi.fn().mockReturnThis(),
    setCardHtml: vi.fn().mockReturnThis(),
    setCardDim: vi.fn().mockReturnThis(),
    setOnCardUpdate: vi.fn().mockReturnThis(),
    setSingleParentEmptyCard: vi.fn().mockReturnThis(),
    updateMainId: vi.fn().mockReturnThis(),
    updateTree: vi.fn().mockReturnThis(),
  };
  return {
    ...mod, // Import and retain default behavior, then override
    default: {
      createChart: vi.fn(() => mockChartInstance),
    },
  };
});

describe('f3.adapter', () => {
  let mockContainer: HTMLDivElement;
  let mockData: any[];
  const mockEmit = vi.fn();
  const mockOnNodeClick = vi.fn(); // NEW: Define mockOnNodeClick here

  beforeEach(() => {
    mockContainer = document.createElement('div');
    document.body.appendChild(mockContainer);
    mockData = [{ id: '1', data: { fullName: 'Test' }, rels: {} }];
    vi.clearAllMocks();
    mockOnNodeClick.mockClear();
  });

  afterEach(() => {
    document.body.removeChild(mockContainer);
  });

  describe('F3Adapter (base implementation)', () => {
    it('should create a chart with correct base settings', () => {
      const chart = F3Adapter.createChart(mockContainer, mockData);

      expect(f3.createChart).toHaveBeenCalledWith(mockContainer, mockData);
      expect(chart.setTransitionTime).toHaveBeenCalledWith(1000);
      expect(chart.setCardXSpacing).toHaveBeenCalledWith(150);
      expect(chart.setCardYSpacing).toHaveBeenCalledWith(150);
    });

    it('should update chart with main ID and options', () => {
      const chart = F3Adapter.createChart(mockContainer, mockData);
      F3Adapter.updateChart(chart, '123', { initial: true });

      expect(chart.updateMainId).toHaveBeenCalledWith('123');
      expect(chart.updateTree).toHaveBeenCalledWith({ initial: true });
    });

    it('should clear chart container', () => {
      mockContainer.innerHTML = '<div>Some content</div>';
      F3Adapter.clearChart(mockContainer);
      expect(mockContainer.innerHTML).toBe('');
    });
  });

  describe('createDefaultF3Adapter', () => {
        it.skip('should create an adapter that sets the card renderer', () => {
          const mockOnNodeClick = vi.fn();
          const adapter = createDefaultF3Adapter(mockEmit, mockOnNodeClick);
          const chart = adapter.createChart(mockContainer, mockData);
    
          expect(chart.setCardHtml).toHaveBeenCalled();
          expect(chart.setCardDim).toHaveBeenCalledWith({ w: 150, h: 200 });
          expect(chart.setOnCardUpdate).toHaveBeenCalled();
    
          const onCardUpdateFunction = chart.setOnCardUpdate.mock.calls[0][0];
          const mockCardData = { data: { id: 'mockId', data: { fullName: 'Mock MemberDto', gender: 'M' }, rels: {} } };
          const mockCardElement = document.createElement('div');
          
          const addEventListenerSpy = vi.spyOn(mockCardElement, 'addEventListener');
    
          // Simulate the onCardUpdate function being called, which sets up the event listener
          onCardUpdateFunction.call(mockCardElement, mockCardData);
          
          const clickHandler = addEventListenerSpy.mock.calls[0][1];
          expect(clickHandler).toBeDefined();
    
          // Manually invoke the click handler
          clickHandler(new MouseEvent('click'));
    
          expect(mockEmit).toHaveBeenCalledWith('show-member-detail-drawer', 'mockId');
          expect(mockOnNodeClick).toHaveBeenCalledWith('mockId', 'Mock MemberDto');
    
          addEventListenerSpy.mockRestore();
        });  });
});