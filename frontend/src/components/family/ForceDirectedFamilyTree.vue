<template>
  <div ref="chartContainer" class="force-directed-graph"></div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, onUnmounted } from 'vue';
import * as d3 from 'd3';
import { useMemberStore } from '@/stores/member.store';
import { useRelationshipStore } from '@/stores/relationship.store';
import type { Member, Relationship } from '@/types';
import { Gender, RelationshipType } from '@/types';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps({
  familyId: { type: String, required: true },
});

const chartContainer = ref<HTMLDivElement | null>(null);
let simulation: d3.Simulation<GraphNode, GraphLink> | null = null;

const memberStore = useMemberStore();
const relationshipStore = useRelationshipStore();

interface GraphNode extends d3.SimulationNodeDatum {
  id: string;
  name: string | undefined;
  gender: string | undefined;
  avatarUrl?: string;
  depth: number; // Re-add depth for forceY
  isRoot?: boolean;
}

interface GraphLink extends d3.SimulationLinkDatum<GraphNode> {
  source: GraphNode | string;
  target: GraphNode | string;
  type: 'parent-child' | 'spouse';
}

const transformData = (members: Member[], relationships: Relationship[]): { nodes: GraphNode[], links: GraphLink[] } => {
  const nodes: GraphNode[] = members.map(m => ({
    id: String(m.id),
    name: m.fullName || `${m.firstName} ${m.lastName}`,
    gender: m.gender,
    avatarUrl: m.avatarUrl,
    depth: -1, // Initialize depth
    isRoot: m.isRoot,
  }));

  const nodeMap = new Map(nodes.map(n => [n.id, n]));

  const links: GraphLink[] = [];
  const spouseLinks = new Set<string>();

  relationships.forEach(rel => {
    const sourceId = String(rel.sourceMemberId);
    const targetId = String(rel.targetMemberId);

    switch (rel.type) {
      case RelationshipType.Husband:
      case RelationshipType.Wife:
        // Ensure spouse links are added only once for a couple
        const linkKey1 = `${sourceId}-${targetId}`;
        const linkKey2 = `${targetId}-${sourceId}`;
        if (!spouseLinks.has(linkKey1) && !spouseLinks.has(linkKey2)) {
          links.push({ source: sourceId, target: targetId, type: 'spouse' });
          spouseLinks.add(linkKey1);
        }
        break;
      case RelationshipType.Father:
      case RelationshipType.Mother:
        links.push({ source: sourceId, target: targetId, type: 'parent-child' });
        break;
    }
  });

  // Calculate depth based on new relationships
  const parentChildRelationships = relationships.filter(rel => rel.type === RelationshipType.Father || rel.type === RelationshipType.Mother);
  const childrenOf = new Map<string, string[]>(); // Map parentId to array of childIds
  const parentsOf = new Map<string, string[]>(); // Map childId to array of parentIds

  parentChildRelationships.forEach(rel => {
    const parentId = String(rel.sourceMemberId);
    const childId = String(rel.targetMemberId);
    if (!childrenOf.has(parentId)) childrenOf.set(parentId, []);
    childrenOf.get(parentId)!.push(childId);

    if (!parentsOf.has(childId)) parentsOf.set(childId, []);
    parentsOf.get(childId)!.push(parentId);
  });

  const roots = nodes.filter(n => !parentsOf.has(n.id));
  const queue: { node: GraphNode; depth: number }[] = roots.map(r => ({ node: r, depth: 0 }));
  const visited = new Set<string>();

  while (queue.length > 0) {
    const { node, depth } = queue.shift()!;
    if (visited.has(node.id)) continue;
    visited.add(node.id);
    node.depth = depth;

    const childrenIds = childrenOf.get(node.id) || [];
    childrenIds.forEach(childId => {
      const childNode = nodeMap.get(childId);
      if (childNode) {
        queue.push({ node: childNode, depth: depth + 1 });
      }
    });
  }

  // Fallback for isolated nodes or those not reached by depth calculation
  nodes.forEach(n => {
    if (n.depth === -1) n.depth = 0; // Assign a default depth
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
    .force('link', d3.forceLink<GraphNode, GraphLink>(links).id(d => d.id).distance(d => d.type === 'spouse' ? 120 : 300).strength(1))
    .force('charge', d3.forceManyBody().strength(-1200))
    .force('collide', d3.forceCollide(67.5))
    .force('x', d3.forceX(width / 2).strength(0.1))
    .force('y', d3.forceY<GraphNode>(d => 150 + d.depth * 240).strength(0.8));

  const link = svg.append('g')
    .attr('stroke-opacity', 0.5)
    .selectAll('line')
    .data(links)
    .join('line')
    .attr('stroke-width', d => d.type === 'spouse' ? 3 : 1.5)
    .attr('stroke', d => d.type === 'spouse' ? 'rgb(var(--v-theme-error))' : 'rgb(var(--v-theme-on-surface))');

  const node = svg.append('g')
    .selectAll('g')
    .data(nodes)
    .join('g')
    .call(drag(simulation) as any);

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
    .attr('fill', d => d.avatarUrl ? `url(#avatar-${d.id})` : (d.gender === Gender.Male ? 'rgb(var(--v-theme-primary))' : 'rgb(var(--v-theme-secondary))'))
    .attr('stroke', d => d.gender === Gender.Male ? 'rgb(var(--v-theme-primary))' : 'rgb(var(--v-theme-secondary))') // Gender-based border
    .attr('stroke-width', 4); // Increased border width

  node.append('text')
    .text(d => d.name || '')
    .attr('x', 0).attr('y', 38)
    .attr('text-anchor', 'middle').attr('font-family', 'sans-serif')
    .attr('font-size', '12px')
    .style('fill', 'rgb(var(--v-theme-on-surface))'); // Theme-aware color

  // --- Legend ---
  const legend = svg.append('g')
    .attr('transform', `translate(20, 20)`);

  const legendData = [
    { color: 'rgb(var(--v-theme-primary))', text: t('member.gender.male'), type: 'circle' },
    { color: 'rgb(var(--v-theme-secondary))', text: t('member.gender.female'), type: 'circle' },
    { color: 'rgb(var(--v-theme-error))', text: t('relationship.type.spouse'), type: 'line' },
    { color: 'rgb(var(--v-theme-on-surface))', text: t('relationship.type.parent_child'), type: 'line' },
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
        .attr('fill', '#fff') // White fill for legend circles
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
    const sourceId = (d.source as GraphNode).id;
    const targetId = (d.target as GraphNode).id;
    linkedByIndex.set(sourceId, linkedByIndex.get(sourceId) || new Set());
    linkedByIndex.set(targetId, linkedByIndex.get(targetId) || new Set());
    linkedByIndex.get(sourceId)!.add(targetId);
    linkedByIndex.get(targetId)!.add(sourceId);
  });

  function isConnected(a: GraphNode, b: GraphNode) {
    return linkedByIndex.get(a.id)?.has(b.id) || a.id === b.id;
  }

  node.on('mouseover', (event, d) => {
    node.transition().duration(200).style('opacity', o => isConnected(d, o) ? 1 : 0.1);
    link.transition().duration(200).style('stroke-opacity', o => {
      const sourceNode = o.source as GraphNode;
      const targetNode = o.target as GraphNode;
      return sourceNode.id === d.id || targetNode.id === d.id ? 1 : 0.1;
    });
  });

  node.on('mouseout', () => {
    node.transition().duration(200).style('opacity', 1);
    link.transition().duration(200).style('stroke-opacity', 0.5);
  });

  simulation.on('tick', () => {
    link
      .attr('x1', d => (d.source as GraphNode).x!)
      .attr('y1', d => (d.source as GraphNode).y!)
      .attr('x2', d => (d.target as GraphNode).x!)
      .attr('y2', d => (d.target as GraphNode).y!);
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
    await Promise.all([
      memberStore.getByFamilyId(props.familyId),
      relationshipStore.getByFamilyId(props.familyId),
    ]);

    const members = memberStore.items;
    const relationships = relationshipStore.items;

    if (members.length > 0) {
      const { nodes, links } = transformData(members, relationships);
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