import f3 from 'family-chart';
import 'family-chart/styles/family-chart.css';

// Define the expected structure for card data used by the f3 chart
interface F3CardData {
  id: string;
  data: {
    avatar?: string;
    gender: 'M' | 'F'; // Strictly M or F
    fullName: string; // Should always be present
    birthYear?: string | number;
    deathYear?: string | number;
    main?: boolean; // Explicitly boolean
    [key: string]: string | number | boolean | undefined; // Allow other keys, including boolean for main
  };
  rels: {
    spouses: string[];
    children: string[];
    father?: string;
    mother?: string;
  };
}

// Define the interface for the F3Adapter
export interface IF3Adapter {
  createChart(container: HTMLElement, data: F3CardData[]): any;
  updateChart(chart: any, mainId: string, options?: { initial?: boolean }): void;
  clearChart(container: HTMLElement): void;
  // Expose other necessary f3 methods or properties through the adapter if needed
}

// Custom Card Renderer function for f3
function CardRenderer(emit: (event: 'show-member-detail-drawer', ...args: any[]) => void) {
  return function (this: HTMLElement, d: { data: F3CardData }) {
    this.innerHTML = `
      <div class="card">
        ${d.data.data.avatar ? getCardInnerImage(d) : getCardInnerText(d)}
      </div>
    `;
    this.addEventListener('click', (e: MouseEvent) => onCardClick(e, d));
  };

  function onCardClick(_: Event, d: { data: F3CardData }) {
    emit('show-member-detail-drawer', d.data.id);
  }

  function getCardInnerImage(d: { data: F3CardData }) {
    return `
      <div class="card-image ${getClassList(d).join(' ')}">
        <img src="${d.data.data.avatar}" />
        <div class="card-label">${d.data.data.fullName || ''}</div>
        <div class="card-dates">${d.data.data.birthYear || ''} - ${d.data.data.deathYear || ''}</div>
      </div>
    `;
  }

  function getCardInnerText(d: { data: F3CardData }) {
    return `
      <div class="card-text ${getClassList(d).join(' ')}">
        <div class="card-label">${d.data.data.fullName || ''}</div>
        <div class="card-dates">${d.data.data.birthYear || ''} - ${d.data.data.deathYear || ''}</div>
      </div>
    `;
  }

  function getClassList(d: { data: F3CardData }) {
    const class_list = [];
    if (d.data.data.gender === 'M') class_list.push('card-male');
    else if (d.data.data.gender === 'F') class_list.push('card-female');
    else class_list.push('card-genderless');

    if (d.data.data.main) class_list.push('card-main');

    return class_list;
  }
}

// Default F3Adapter implementation
export const F3Adapter: IF3Adapter = {
  createChart(container: HTMLElement, data: F3CardData[]): any {
    const chart = f3
      .createChart(container, data)
      .setTransitionTime(1000)
      .setCardXSpacing(150)
      .setCardYSpacing(150)
      .setSingleParentEmptyCard(false, {label: 'Unknown'})
      ;

    return chart;
  },

  updateChart(chart: any, mainId: string, options?: { initial?: boolean }): void {
    if (chart) {
      chart.updateMainId(mainId);
      chart.updateTree(options);
    }
  },

  clearChart(container: HTMLElement): void {
    if (container) {
      container.innerHTML = '';
    }
  },
};

// This function provides the default F3Adapter with the emit function for card rendering
export function createDefaultF3Adapter(emit: (event: 'show-member-detail-drawer', ...args: any[]) => void): IF3Adapter {
  const adapter = { ...F3Adapter }; // Create a copy to avoid modifying the base adapter directly
  // Override createChart to set the custom card renderer
  adapter.createChart = (container: HTMLElement, data: F3CardData[]): any => {
    const chart = f3
      .createChart(container, data)
      .setTransitionTime(1000)
      .setCardXSpacing(150)
      .setCardYSpacing(150)
      .setSingleParentEmptyCard(false, {label: 'Unknown'})
      ;

    chart.setCardHtml()
         .setCardDim({ w: 150, h: 200 })
         .setOnCardUpdate(CardRenderer(emit)); // Inject emit into the CardRenderer

    return chart;
  };
  return adapter;
}
