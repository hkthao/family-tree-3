import type { CombinedAiContentDto, CardData, FamilyDto, MemberDto, EventDto } from '@/types';

export const mapCombinedAiContentToCardData = (
  combinedContent: CombinedAiContentDto,
  familyId: string // Pass familyId as a parameter
): CardData[] => {
  const cards: CardData[] = [];
  let idCounter = 1;

  combinedContent.families?.forEach((family: FamilyDto) => {
    cards.push({
      id: (idCounter++).toString(),
      type: 'Family',
      title: family.name,
      summary: family.description || '',
      data: {
        ...family,
        familyId: family.id, // Assuming family.id is the familyId
      } as FamilyDto,
    });
  });

  combinedContent.members?.forEach((member: MemberDto) => {
    cards.push({
      id: (idCounter++).toString(),
      type: 'Member',
      title: `${member.firstName} ${member.lastName}`,
      summary: `${member.dateOfBirth ? new Date(member.dateOfBirth).getFullYear() : ''} - ${member.dateOfDeath ? new Date(member.dateOfDeath).getFullYear() : ''}`.trim(),
      data: {
        ...member,
        familyId: member.familyId || familyId, // Default to familyId if not provided by AI
        dateOfBirth: member.dateOfBirth ? new Date(member.dateOfBirth) : undefined,
        dateOfDeath: member.dateOfDeath ? new Date(member.dateOfDeath) : undefined,
      } as MemberDto, // Store the full member object with parsed dates
    });
  });

  combinedContent.events?.forEach((event: EventDto) => {
    cards.push({
      id: (idCounter++).toString(),
      type: 'Event',
      title: event.name,
      summary: `${event.solarDate ? new Date(event.solarDate).toLocaleDateString() : ''} - ${event.description || event.name}`.trim(),
      data: {
        ...event,
        familyId: event.familyId || familyId, // Default to familyId if not provided by AI
        solarDate: event.solarDate ? new Date(event.solarDate) : undefined,
      } as EventDto, // Store the full event object with parsed dates
    });
  });

  return cards;
};