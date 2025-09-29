<template>
  <div ref="chartContainer" class="force-directed-graph"></div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, onUnmounted } from 'vue';
import * as d3 from 'd3';
import { useMemberStore } from '@/stores/member.store';
import type { Member } from '@/types/family';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps({
  familyId: { type: String, required: true },
});

const chartContainer = ref<HTMLDivElement | null>(null);
let simulation: d3.Simulation<GraphNode, GraphLink> | null = null;

const memberStore = useMemberStore();

interface GraphNode extends d3.SimulationNodeDatum {
  id: string;
  name: string;
  gender: string;
  avatarUrl?: string;
  depth: number;
}

interface GraphLink extends d3.SimulationLinkDatum<GraphNode> {
  source: GraphNode;
  target: GraphNode;
  type: 'parent-child' | 'spouse';
}

const transformData = (members: Member[]): { nodes: GraphNode[], links: GraphLink[] } => {
  const nodes: GraphNode[] = members.map(m => ({
    id: String(m.id),
    name: m.fullName,
    gender: m.gender,
    avatarUrl: m.avatarUrl,
    depth: -1, // Initialize depth
  }));

  const nodeMap = new Map(nodes.map(n => [n.id, n]));

  // Calculate depth
  const roots = nodes.filter(n => !members.find(m => m.id === n.id)?.fatherId && !members.find(m => m.id === n.id)?.motherId);
  const queue: { node: GraphNode; depth: number }[] = roots.map(r => ({ node: r, depth: 0 }));
  const visited = new Set(roots.map(r => r.id));

  while (queue.length > 0) {
    const { node, depth } = queue.shift()!;
    node.depth = depth;

    const children = members.filter(m => String(m.fatherId) === node.id || String(m.motherId) === node.id);
    children.forEach(childData => {
      const childNode = nodeMap.get(String(childData.id));
      if (childNode && !visited.has(childNode.id)) {
        visited.add(childNode.id);
        queue.push({ node: childNode, depth: depth + 1 });
      }
    });
  }
  // Assign depth to nodes that might have been missed (e.g., spouses of deep nodes)
  nodes.forEach(n => {
    if (n.depth === -1) n.depth = 0; // Fallback for isolated nodes
  });

  const links: any[] = [];
  const spouseLinks = new Set();

  members.forEach(m => {
    const sourceId = String(m.id);
    if (m.fatherId) links.push({ source: sourceId, target: String(m.fatherId), type: 'parent-child' });
    if (m.motherId) links.push({ source: sourceId, target: String(m.motherId), type: 'parent-child' });
    if (m.spouseId) {
      const targetId = String(m.spouseId);
      const linkKey1 = `${sourceId}-${targetId}`;
      const linkKey2 = `${targetId}-${sourceId}`;
      if (!spouseLinks.has(linkKey1) && !spouseLinks.has(linkKey2)) {
        links.push({ source: sourceId, target: targetId, type: 'spouse' });
        spouseLinks.add(linkKey1);
      }
    }
  });

  return { nodes, links };
};

const renderChart = (nodes: GraphNode[], links: GraphLink[]) => {
  if (!chartContainer.value) return;

  const width = chartContainer.value.clientWidth;
  const height = chartContainer.value.clientHeight || 800;

  d3.select(chartContainer.value).select('svg').remove();

  const svg = d3.select(chartContainer.value).append('svg')
    .attr('width', width)
    .attr('height', height)
    .attr('viewBox', [0, 0, width, height]);

  simulation = d3.forceSimulation(nodes)
    .force('link', d3.forceLink<GraphNode, GraphLink>(links).id(d => d.id).distance(d => d.type === 'spouse' ? 80 : 200).strength(1))
    .force('charge', d3.forceManyBody().strength(-800))
    .force('collide', d3.forceCollide(45))
    .force('x', d3.forceX(width / 2).strength(0.1))
    .force('y', d3.forceY<GraphNode>(d => 150 + d.depth * 240).strength(0.8));

  const link = svg.append('g')
    .attr('stroke-opacity', 0.5)
    .selectAll('line')
    .data(links)
    .join('line')
    .attr('stroke-width', d => d.type === 'spouse' ? 3 : 1.5)
    .attr('stroke', d => d.type === 'spouse' ? '#ff4081' : '#999');

  const node = svg.append('g')
    .selectAll('g')
    .data(nodes)
    .join('g')
    .call(drag(simulation));

  const defs = svg.append('defs');
  nodes.forEach(d => {
    if (d.avatarUrl) {
      defs.append('pattern').attr('id', `avatar-${d.id}`)
        .attr('height', 1).attr('width', 1)
        .append('image').attr('x', 0).attr('y', 0)
        .attr('height', 50).attr('width', 50).attr('href', d.avatarUrl);
    }
  });

  node.append('circle')
    .attr('r', 25)
    .attr('fill', d => d.avatarUrl ? `url(#avatar-${d.id})` : (d.gender === 'Male' ? '#81d4fa' : '#f48fb1'))
    .attr('stroke', d => d.gender === 'Male' ? '#81d4fa' : '#f48fb1') // Gender-based border
    .attr('stroke-width', 4); // Increased border width

  node.append('text')
    .text(d => d.name)
    .attr('x', 0).attr('y', 38)
    .attr('text-anchor', 'middle').attr('font-family', 'sans-serif')
    .attr('font-size', '12px')
    .style('fill', 'rgb(var(--v-theme-on-surface))'); // Theme-aware color

  // --- Legend ---
  const legend = svg.append('g')
    .attr('transform', `translate(20, 20)`);

  const legendData = [
    { color: '#81d4fa', text: t('member.gender.male'), type: 'circle' },
    { color: '#f48fb1', text: t('member.gender.female'), type: 'circle' },
    { color: '#ff4081', text: t('relationship.type.spouse'), type: 'line' },
    { color: '#999', text: t('relationship.type.parent') + '-' + t('relationship.type.child'), type: 'line' },
  ];

  const legendItem = legend.selectAll('.legend-item')
    .data(legendData)
    .join('g')
    .attr('class', 'legend-item')
    .attr('transform', (d, i) => `translate(0, ${i * 25})`);

  legendItem.each(function(d) {
    const g = d3.select(this);
    if (d.type === 'circle') {
      g.append('circle')
        .attr('r', 8)
        .attr('cx', 0)
        .attr('cy', 0)
        .attr('fill', d.color === '#81d4fa' ? '#fff' : '#fff') // White fill for legend circles
        .attr('stroke', d.color)
        .attr('stroke-width', 3);
    } else {
      g.append('line')
        .attr('x1', -8)
        .attr('y1', 0)
        .attr('x2', 8)
        .attr('y2', 0)
        .attr('stroke', d.color)
        .attr('stroke-width', 3);
    }
    g.append('text')
      .attr('x', 15)
      .attr('y', 5)
      .style('fill', 'rgb(var(--v-theme-on-surface))')
      .text(d.text);
  });

  // --- Highlighting Interaction ---
  const linkedByIndex = new Map<string, Set<string>>();
  links.forEach(d => {
    linkedByIndex.set(d.source.id, linkedByIndex.get(d.source.id) || new Set());
    linkedByIndex.set(d.target.id, linkedByIndex.get(d.target.id) || new Set());
    linkedByIndex.get(d.source.id)!.add(d.target.id);
    linkedByIndex.get(d.target.id)!.add(d.source.id);
  });

  function isConnected(a: GraphNode, b: GraphNode) {
    return linkedByIndex.get(a.id)?.has(b.id) || a.id === b.id;
  }

  node.on('mouseover', (event, d) => {
    node.transition().duration(200).style('opacity', o => isConnected(d, o) ? 1 : 0.1);
    link.transition().duration(200).style('stroke-opacity', o => o.source.id === d.id || o.target.id === d.id ? 1 : 0.1);
  });

  node.on('mouseout', () => {
    node.transition().duration(200).style('opacity', 1);
    link.transition().duration(200).style('stroke-opacity', 0.5);
  });

  simulation.on('tick', () => {
    link
      .attr('x1', d => d.source.x!)
      .attr('y1', d => d.source.y!)
      .attr('x2', d => d.target.x!)
      .attr('y2', d => d.target.y!);
    node.attr('transform', d => `translate(${d.x}, ${d.y})`);
  });

  function drag(sim: d3.Simulation<GraphNode, undefined>) {
    function dragstarted(event: d3.D3DragEvent<SVGGElement, GraphNode, GraphNode>) {
      if (!event.active) sim.alphaTarget(0.3).restart();
      event.subject.fx = event.subject.x;
      event.subject.fy = event.subject.y;
    }
    function dragged(event: d3.D3DragEvent<SVGGElement, GraphNode, GraphNode>) {
      event.subject.fx = event.x;
      event.subject.fy = event.y;
    }
    function dragended(event: d3.D3DragEvent<SVGGElement, GraphNode, GraphNode>) {
      if (!event.active) sim.alphaTarget(0);
      event.subject.fx = null;
      event.subject.fy = null;
    }
    return d3.drag<SVGGElement, GraphNode>()
      .on('start', dragstarted).on('drag', dragged).on('end', dragended);
  }
};

const initialize = async () => {
  if (props.familyId) {
    const members = await memberStore.getMembersByFamilyId(props.familyId);
    if (members && members.length > 0) {
      const { nodes, links } = transformData(members);
      renderChart(nodes, links);
    } else {
       if (!chartContainer.value) return;
       d3.select(chartContainer.value).select('svg').remove();
       d3.select(chartContainer.value).html(`<div class="empty-message">${t('familyTree.noMembersMessage')}</div>`);
    }
  }
};

onMounted(() => {
  initialize();
});

watch(() => props.familyId, () => {
  initialize();
});

onUnmounted(() => {
  if (simulation) {
    simulation.stop();
  }
});

</script>

<style>
.force-directed-graph {
  width: 100%;
  height: 80vh;
}
.empty-message {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  flex-direction: column;
  height: 80vh;
}
</style>