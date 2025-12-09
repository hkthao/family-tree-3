using backend.Domain.Enums;
using backend.Domain.Entities; // Added
using backend.Domain.ValueObjects;

namespace backend.Infrastructure.Services;

public static class RuleDefinitions
{
    public static List<RelationshipRule> GetRules()
    {
        var rules = new List<RelationshipRule>();

        // Helper to get member by ID safely
        Func<IReadOnlyDictionary<Guid, Member>, Guid, Member?> getMember =
            (membersDict, memberId) => membersDict.TryGetValue(memberId, out var m) ? m : null;

        // 1. Direct Relationships (5 rules)
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Father }),
            (path, membersDict) => path.Edges.Count == 1, // Only 1 edge, and it's Father
            "cha"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Mother }),
            (path, membersDict) => path.Edges.Count == 1, // Only 1 edge, and it's Mother
            "mẹ"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Husband }),
            (path, membersDict) => path.Edges.Count == 1, // Only 1 edge, and it's Husband
            "chồng"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Wife }),
            (path, membersDict) => path.Edges.Count == 1, // Only 1 edge, and it's Wife
            "vợ"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Child }),
            (path, membersDict) => path.Edges.Count == 1, // Only 1 edge, and it's Child
            "con"
        ));

        // 2. Grandparents (4 rules for A to B, where B is grandparent)
        // Pattern: A -> Parent (Child), Parent -> Grandparent (Child) => [Child, Child]
        // Condition checks gender of the Grandparent (B) and the intermediate Parent
        // Path: A (NodeIds[0]) -> Intermediate Parent (NodeIds[1]) -> B (NodeIds[2])
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Child, RelationshipType.Child }),
            (path, membersDict) => {
                var grandParent = getMember(membersDict, path.NodeIds.Last()); // B
                var parent = getMember(membersDict, path.NodeIds[path.NodeIds.Count - 2]); // Intermediate Parent
                return grandParent != null && parent != null &&
                       grandParent.Gender == Gender.Male.ToString() && parent.Gender == Gender.Male.ToString(); // A is child of Male parent, Male parent is child of Male grandparent
            },
            "ông nội"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Child, RelationshipType.Child }),
            (path, membersDict) => {
                var grandParent = getMember(membersDict, path.NodeIds.Last());
                var parent = getMember(membersDict, path.NodeIds[path.NodeIds.Count - 2]);
                return grandParent != null && parent != null &&
                       grandParent.Gender == Gender.Female.ToString() && parent.Gender == Gender.Male.ToString(); // A is child of Male parent, Male parent is child of Female grandparent
            },
            "bà nội"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Child, RelationshipType.Child }),
            (path, membersDict) => {
                var grandParent = getMember(membersDict, path.NodeIds.Last());
                var parent = getMember(membersDict, path.NodeIds[path.NodeIds.Count - 2]);
                return grandParent != null && parent != null &&
                       grandParent.Gender == Gender.Male.ToString() && parent.Gender == Gender.Female.ToString(); // A is child of Female parent, Female parent is child of Male grandparent
            },
            "ông ngoại"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Child, RelationshipType.Child }),
            (path, membersDict) => {
                var grandParent = getMember(membersDict, path.NodeIds.Last());
                var parent = getMember(membersDict, path.NodeIds[path.NodeIds.Count - 2]);
                return grandParent != null && parent != null &&
                       grandParent.Gender == Gender.Female.ToString() && parent.Gender == Gender.Female.ToString(); // A is child of Female parent, Female parent is child of Female grandparent
            },
            "bà ngoại"
        ));

        // 3. Grandchildren (4 rules for A to B, where A is grandparent and B is grandchild)
        // Path: Grandparent (A) -> Parent (P) -> Grandchild (B)
        // Pattern: [Father, Father], [Father, Mother], [Mother, Father], [Mother, Mother]
        // The edge types now reflect the direct relationship from source to target.

        // A (Grandparent) -> B (Grandchild) through a male child (Paternal side)
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Father, RelationshipType.Father }), // A is father of P, P is father of B
            (path, membersDict) => {
                var grandparent = getMember(membersDict, path.NodeIds.First()); // A
                var parent = getMember(membersDict, path.NodeIds[1]); // P
                var grandchild = getMember(membersDict, path.NodeIds.Last()); // B
                return grandparent != null && parent != null && grandchild != null &&
                       grandparent.Gender == Gender.Male.ToString(); // Grandfather via male child
            },
            "cháu nội" // Grandchild of paternal grandfather
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Mother, RelationshipType.Father }), // A is mother of P, P is father of B
            (path, membersDict) => {
                var grandparent = getMember(membersDict, path.NodeIds.First()); // A
                var parent = getMember(membersDict, path.NodeIds[1]); // P
                var grandchild = getMember(membersDict, path.NodeIds.Last()); // B
                return grandparent != null && parent != null && grandchild != null &&
                       grandparent.Gender == Gender.Female.ToString(); // Grandmother via male child
            },
            "cháu nội" // Grandchild of paternal grandmother
        ));

        // A (Grandparent) -> B (Grandchild) through a female child (Maternal side)
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Father, RelationshipType.Mother }), // A is father of P, P is mother of B
            (path, membersDict) => {
                var grandparent = getMember(membersDict, path.NodeIds.First()); // A
                var parent = getMember(membersDict, path.NodeIds[1]); // P
                var grandchild = getMember(membersDict, path.NodeIds.Last()); // B
                return grandparent != null && parent != null && grandchild != null &&
                       grandparent.Gender == Gender.Male.ToString(); // Grandfather via female child
            },
            "cháu ngoại" // Grandchild of maternal grandfather
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Mother, RelationshipType.Mother }), // A is mother of P, P is mother of B
            (path, membersDict) => {
                var grandparent = getMember(membersDict, path.NodeIds.First()); // A
                var parent = getMember(membersDict, path.NodeIds[1]); // P
                var grandchild = getMember(membersDict, path.NodeIds.Last()); // B
                return grandparent != null && parent != null && grandchild != null &&
                       grandparent.Gender == Gender.Female.ToString(); // Grandmother via female child
            },
            "cháu ngoại" // Grandchild of maternal grandmother
        ));

        // 4. Siblings (4 rules for A to B, where B is sibling)
        // Path A -> Common Parent (Child), Common Parent -> B (Father/Mother) => [Child, Father] or [Child, Mother]
        // This pattern correctly identifies siblings through a common parent.
        // A (NodeIds[0]) -> Common Parent (NodeIds[1]) -> B (NodeIds[2])
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Child, RelationshipType.Father }),
            (path, membersDict) => {
                var commonParent = getMember(membersDict, path.NodeIds[1]);
                var B = getMember(membersDict, path.NodeIds[2]);
                return commonParent != null && B != null && commonParent.Id == path.Edges[0].TargetMemberId && path.Edges[1].SourceMemberId == commonParent.Id;
            },
            "anh/chị/em (cùng cha)"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Child, RelationshipType.Mother }),
            (path, membersDict) => {
                var commonParent = getMember(membersDict, path.NodeIds[1]);
                var B = getMember(membersDict, path.NodeIds[2]);
                return commonParent != null && B != null && commonParent.Id == path.Edges[0].TargetMemberId && path.Edges[1].SourceMemberId == commonParent.Id;
            },
            "anh/chị/em (cùng mẹ)"
        ));
        // Add more sibling rules if needed to differentiate older/younger, male/female using additional conditions

        // 5. Uncles/Aunts (paternal side) (4 rules)
        // A -> Grandparent (Child), Grandparent -> Uncle/Aunt (Father/Mother) -> Uncle/Aunt (Child)
        // This is tricky. Let's simplify and use the common logic.
        // Path for A to B (Uncle/Aunt): A -> Parent (Child), Parent -> Grandparent (Child), Grandparent -> Uncle/Aunt (Father/Mother)
        // Pattern: [Child, Child, Father] or [Child, Child, Mother]
        // A (NodeIds[0]) -> Parent (NodeIds[1]) -> Grandparent (NodeIds[2]) -> B (NodeIds[3])
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Child, RelationshipType.Child, RelationshipType.Father }),
            (path, membersDict) => {
                var grandParent = getMember(membersDict, path.NodeIds[2]);
                var uncleAunt = getMember(membersDict, path.NodeIds[3]);
                return grandParent != null && uncleAunt != null &&
                       grandParent.Gender == Gender.Male.ToString() && uncleAunt.Gender == Gender.Male.ToString(); // Paternal Uncle (Bác/Chú)
            },
            "bác/chú"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Child, RelationshipType.Child, RelationshipType.Mother }),
            (path, membersDict) => {
                var grandParent = getMember(membersDict, path.NodeIds[2]);
                var uncleAunt = getMember(membersDict, path.NodeIds[3]);
                return grandParent != null && uncleAunt != null &&
                       grandParent.Gender == Gender.Male.ToString() && uncleAunt.Gender == Gender.Female.ToString(); // Paternal Aunt (Cô)
            },
            "cô"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Child, RelationshipType.Child, RelationshipType.Father }),
            (path, membersDict) => {
                var grandParent = getMember(membersDict, path.NodeIds[2]);
                var uncleAunt = getMember(membersDict, path.NodeIds[3]);
                return grandParent != null && uncleAunt != null &&
                       grandParent.Gender == Gender.Female.ToString() && uncleAunt.Gender == Gender.Male.ToString(); // Maternal Uncle (Cậu)
            },
            "cậu"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Child, RelationshipType.Child, RelationshipType.Mother }),
            (path, membersDict) => {
                var grandParent = getMember(membersDict, path.NodeIds[2]);
                var uncleAunt = getMember(membersDict, path.NodeIds[3]);
                return grandParent != null && uncleAunt != null &&
                       grandParent.Gender == Gender.Female.ToString() && uncleAunt.Gender == Gender.Female.ToString(); // Maternal Aunt (Dì)
            },
            "dì"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Child, RelationshipType.Child, RelationshipType.Mother }),
            (path, membersDict) => {
                var grandParent = getMember(membersDict, path.NodeIds[2]);
                var uncleAunt = getMember(membersDict, path.NodeIds[3]);
                return grandParent != null && uncleAunt != null &&
                       grandParent.Gender == Gender.Female.ToString() && uncleAunt.Gender == Gender.Male.ToString(); // Maternal Uncle (Cậu)
            },
            "cậu" // Nephew/Niece to Maternal Uncle
        ));


        // 6. Niece/Nephew (from A to B) - inverse of Uncles/Aunts (4 rules)
        // Path from A (Niece/Nephew) to B (Uncle/Aunt): A -> Parent (Child), Parent -> Sibling of Uncle/Aunt (Child), Sibling -> B (Father/Mother)
        // Pattern: [Father, Father, Child] or [Mother, Mother, Child]
        // This is complex, let's use the inverse of the above
        // A -> Sibling's Parent (Father/Mother), Sibling's Parent -> B (Father/Mother)
        // Path A -> Parent (Child), Parent -> Uncle/Aunt (Child).
        // Pattern: [Child, Child, Father] or [Child, Child, Mother] -- this is the same as Uncle/Aunt rules.
        // The interpretation changes depending on direction.
        // Let's use the inverse pattern of Uncle/Aunt.
        // Inverse for A to B where B is Niece/Nephew
        // A -> B (Father/Mother of A's child), B -> C (Child of A)
        // Path: A (NodeIds[0]) -> Sibling (NodeIds[1]) -> B (NodeIds[2])
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Father, RelationshipType.Father, RelationshipType.Child }),
            (path, membersDict) => {
                var grandParent = getMember(membersDict, path.NodeIds[1]);
                var nephewNiece = getMember(membersDict, path.NodeIds.Last());
                return grandParent != null && nephewNiece != null &&
                       grandParent.Gender == Gender.Male.ToString(); // A is Male, and B is his nephew/niece from Male sibling
            },
            "cháu"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Father, RelationshipType.Mother, RelationshipType.Child }),
            (path, membersDict) => {
                var grandParent = getMember(membersDict, path.NodeIds[1]);
                var nephewNiece = getMember(membersDict, path.NodeIds.Last());
                return grandParent != null && nephewNiece != null &&
                       grandParent.Gender == Gender.Female.ToString(); // A is Male, and B is his nephew/niece from Female sibling
            },
            "cháu"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Mother, RelationshipType.Father, RelationshipType.Child }),
            (path, membersDict) => {
                var grandParent = getMember(membersDict, path.NodeIds[1]);
                var nephewNiece = getMember(membersDict, path.NodeIds.Last());
                return grandParent != null && nephewNiece != null &&
                       grandParent.Gender == Gender.Male.ToString(); // A is Female, and B is her nephew/niece from Male sibling
            },
            "cháu"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Mother, RelationshipType.Mother, RelationshipType.Child }),
            (path, membersDict) => {
                var grandParent = getMember(membersDict, path.NodeIds[1]);
                var nephewNiece = getMember(membersDict, path.NodeIds.Last());
                return grandParent != null && nephewNiece != null &&
                       grandParent.Gender == Gender.Female.ToString(); // A is Female, and B is her nephew/niece from Female sibling
            },
            "cháu"
        ));


        // Total so far: 5 (direct) + 4 (grandparent) + 4 (grandchild) + 2 (siblings) + 4 (uncle/aunt) + 4 (nephew/niece) = 23 rules.
        // Need to add some in-law relationships and more specific sibling/uncle/aunt rules.

        // In-laws (from A to B)
        // A -> Spouse (Husband/Wife), Spouse -> Parent (Child)
        // Pattern: [Husband/Wife, Child]
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Husband, RelationshipType.Child }),
            (path, membersDict) => {
                var spouse = getMember(membersDict, path.NodeIds[1]);
                var parentInLaw = getMember(membersDict, path.NodeIds[2]);
                return spouse != null && parentInLaw != null && parentInLaw.Gender == Gender.Male.ToString();
            },
            "bố vợ"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Husband, RelationshipType.Child }),
            (path, membersDict) => {
                var spouse = getMember(membersDict, path.NodeIds[1]);
                var parentInLaw = getMember(membersDict, path.NodeIds[2]);
                return spouse != null && parentInLaw != null && parentInLaw.Gender == Gender.Female.ToString();
            },
            "mẹ vợ"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Wife, RelationshipType.Child }),
            (path, membersDict) => {
                var spouse = getMember(membersDict, path.NodeIds[1]);
                var parentInLaw = getMember(membersDict, path.NodeIds[2]);
                return spouse != null && parentInLaw != null && parentInLaw.Gender == Gender.Male.ToString();
            },
            "bố chồng"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Wife, RelationshipType.Child }),
            (path, membersDict) => {
                var spouse = getMember(membersDict, path.NodeIds[1]);
                var parentInLaw = getMember(membersDict, path.NodeIds[2]);
                return spouse != null && parentInLaw != null && parentInLaw.Gender == Gender.Female.ToString();
            },
            "mẹ chồng"
        ));

        // Son-in-law/Daughter-in-law (from A to B)
        // A -> Child (Father/Mother), Child -> Spouse (Husband/Wife)
        // Pattern: [Father/Mother, Husband/Wife]
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Father, RelationshipType.Husband }),
            (path, membersDict) => {
                var child = getMember(membersDict, path.NodeIds[1]);
                var sonInLaw = getMember(membersDict, path.NodeIds[2]);
                return child != null && sonInLaw != null && child.Gender == Gender.Female.ToString(); // B is husband of A's daughter
            },
            "con rể"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Mother, RelationshipType.Husband }),
            (path, membersDict) => {
                var child = getMember(membersDict, path.NodeIds[1]);
                var sonInLaw = getMember(membersDict, path.NodeIds[2]);
                return child != null && sonInLaw != null && child.Gender == Gender.Female.ToString(); // B is husband of A's daughter
            },
            "con rể"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Father, RelationshipType.Wife }),
            (path, membersDict) => {
                var child = getMember(membersDict, path.NodeIds[1]);
                var daughterInLaw = getMember(membersDict, path.NodeIds[2]);
                return child != null && daughterInLaw != null && child.Gender == Gender.Male.ToString(); // B is wife of A's son
            },
            "con dâu"
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Mother, RelationshipType.Wife }),
            (path, membersDict) => {
                var child = getMember(membersDict, path.NodeIds[1]);
                var daughterInLaw = getMember(membersDict, path.NodeIds[2]);
                return child != null && daughterInLaw != null && child.Gender == Gender.Male.ToString(); // B is wife of A's son
            },
            "con dâu"
        ));

        // 7. Cousins (Anh họ / Em họ) - Path A -> B where A and B are cousins
        // Path A -> ParentA (Child), ParentA -> Grandparent (Child), Grandparent -> ParentB (Father/Mother), ParentB -> B (Child)
        // A (NodeIds[0]), ParentA (NodeIds[1]), Grandparent (NodeIds[2]), ParentB (NodeIds[3]), B (NodeIds[4])

        // Paternal Cousin (Anh họ/Em họ)
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Child, RelationshipType.Child, RelationshipType.Father, RelationshipType.Child }), // A->PA->GP->PB->B
            (path, membersDict) => {
                if (path.NodeIds.Count != 5) return false;
                var A = getMember(membersDict, path.NodeIds[0]);
                var ParentA = getMember(membersDict, path.NodeIds[1]);
                var Grandparent = getMember(membersDict, path.NodeIds[2]);
                var ParentB = getMember(membersDict, path.NodeIds[3]);
                var B = getMember(membersDict, path.NodeIds[4]);

                return A != null && ParentA != null && Grandparent != null && ParentB != null && B != null &&
                       Grandparent.Gender == Gender.Male.ToString(); // Common paternal grandparent
            },
            "anh/chị/em họ (bên nội)" // General paternal cousin
        ));

        // Maternal Cousin (Anh họ/Em họ)
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Child, RelationshipType.Child, RelationshipType.Mother, RelationshipType.Child }), // A->PA->GP->PB->B
            (path, membersDict) => {
                if (path.NodeIds.Count != 5) return false;
                var A = getMember(membersDict, path.NodeIds[0]);
                var ParentA = getMember(membersDict, path.NodeIds[1]);
                var Grandparent = getMember(membersDict, path.NodeIds[2]);
                var ParentB = getMember(membersDict, path.NodeIds[3]);
                var B = getMember(membersDict, path.NodeIds[4]);

                return A != null && ParentA != null && Grandparent != null && ParentB != null && B != null &&
                       Grandparent.Gender == Gender.Female.ToString(); // Common maternal grandparent
            },
            "anh/chị/em họ (bên ngoại)" // General maternal cousin
        ));

        // Great-Uncles/Aunts (Ông bác/Ông chú/Bà cô/Bà dì) from Great-Nephew/Niece to Great-Uncle/Aunt
        // Path: A (Great-Nephew/Niece) -> Parent -> Grandparent -> Great-Grandparent -> Great-Uncle/Aunt
        // Pattern: [Child, Child, Child, Father] or [Child, Child, Child, Mother]
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Child, RelationshipType.Child, RelationshipType.Child, RelationshipType.Father }),
            (path, membersDict) => {
                if (path.NodeIds.Count != 5) return false;
                var A = getMember(membersDict, path.NodeIds[0]); // Great-Nephew/Niece
                var greatGrandparent = getMember(membersDict, path.NodeIds[3]); // Z
                var greatUncleAunt = getMember(membersDict, path.NodeIds[4]); // B

                return A != null && greatGrandparent != null && greatUncleAunt != null &&
                       greatGrandparent.Gender == Gender.Male.ToString() && greatUncleAunt.Gender == Gender.Male.ToString();
            },
            "cố/ông bác/ông chú" // Great-Nephew/Niece to Paternal Great-Uncle
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Child, RelationshipType.Child, RelationshipType.Child, RelationshipType.Mother }),
            (path, membersDict) => {
                if (path.NodeIds.Count != 5) return false;
                var A = getMember(membersDict, path.NodeIds[0]); // Great-Nephew/Niece
                var greatGrandparent = getMember(membersDict, path.NodeIds[3]); // Z
                var greatUncleAunt = getMember(membersDict, path.NodeIds[4]); // B

                return A != null && greatGrandparent != null && greatUncleAunt != null &&
                       greatGrandparent.Gender == Gender.Male.ToString() && greatUncleAunt.Gender == Gender.Female.ToString();
            },
            "cố/bà cô" // Great-Nephew/Niece to Paternal Great-Aunt
        ));

        // Add rules for maternal great-uncle/aunt if needed
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Child, RelationshipType.Child, RelationshipType.Child, RelationshipType.Father }),
            (path, membersDict) => {
                if (path.NodeIds.Count != 5) return false;
                var A = getMember(membersDict, path.NodeIds[0]); // Great-Nephew/Niece
                var greatGrandparent = getMember(membersDict, path.NodeIds[3]); // Z
                var greatUncleAunt = getMember(membersDict, path.NodeIds[4]); // B

                return A != null && greatGrandparent != null && greatUncleAunt != null &&
                       greatGrandparent.Gender == Gender.Female.ToString() && greatUncleAunt.Gender == Gender.Male.ToString();
            },
            "cố/ông cậu" // Great-Nephew/Niece to Maternal Great-Uncle
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Child, RelationshipType.Child, RelationshipType.Child, RelationshipType.Mother }),
            (path, membersDict) => {
                if (path.NodeIds.Count != 5) return false;
                var A = getMember(membersDict, path.NodeIds[0]); // Great-Nephew/Niece
                var greatGrandparent = getMember(membersDict, path.NodeIds[3]); // Z
                var greatUncleAunt = getMember(membersDict, path.NodeIds[4]); // B

                return A != null && greatGrandparent != null && greatUncleAunt != null &&
                       greatGrandparent.Gender == Gender.Female.ToString() && greatUncleAunt.Gender == Gender.Female.ToString();
            },
            "cố/bà dì" // Great-Nephew/Niece to Maternal Great-Aunt
        ));

        // Great-Nephew/Niece (Chắt) from Great-Uncle/Aunt to Great-Nephew/Niece
        // Path: B (Great-Uncle/Aunt) -> Parent -> Grandparent -> Great-Grandparent -> A (Great-Nephew/Niece)
        // Pattern: [Father, Father, Father, Child] or similar (4 edges)

        // B is Cố/Ông bác/Ông chú (Paternal Great-Uncle) of A
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Father, RelationshipType.Father, RelationshipType.Father, RelationshipType.Child }),
            (path, membersDict) => {
                if (path.NodeIds.Count != 5) return false;
                var greatUncleAunt = getMember(membersDict, path.NodeIds[0]); // B
                var greatGrandparent = getMember(membersDict, path.NodeIds[1]); // Z's parent
                var grandparent = getMember(membersDict, path.NodeIds[2]); // Y's parent
                var parent = getMember(membersDict, path.NodeIds[3]); // X's parent
                var greatNephewNiece = getMember(membersDict, path.NodeIds[4]); // A

                return greatUncleAunt != null && greatGrandparent != null && grandparent != null && parent != null && greatNephewNiece != null &&
                       greatUncleAunt.Gender == Gender.Male.ToString() && // B is male
                       greatGrandparent.Gender == Gender.Male.ToString(); // Through paternal male line
            },
            "chắt" // Great-nephew/niece of paternal great-uncle
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Mother, RelationshipType.Father, RelationshipType.Father, RelationshipType.Child }),
            (path, membersDict) => {
                if (path.NodeIds.Count != 5) return false;
                var greatUncleAunt = getMember(membersDict, path.NodeIds[0]); // B
                var greatGrandparent = getMember(membersDict, path.NodeIds[1]); // Z's parent
                var grandparent = getMember(membersDict, path.NodeIds[2]); // Y's parent
                var parent = getMember(membersDict, path.NodeIds[3]); // X's parent
                var greatNephewNiece = getMember(membersDict, path.NodeIds[4]); // A

                return greatUncleAunt != null && greatGrandparent != null && grandparent != null && parent != null && greatNephewNiece != null &&
                       greatUncleAunt.Gender == Gender.Female.ToString() && // B is female
                       greatGrandparent.Gender == Gender.Male.ToString(); // Through paternal male line
            },
            "chắt" // Great-nephew/niece of paternal great-aunt
        ));
        // Add similar rules for maternal great-uncles/aunts
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Father, RelationshipType.Mother, RelationshipType.Mother, RelationshipType.Child }),
            (path, membersDict) => {
                if (path.NodeIds.Count != 5) return false;
                var greatUncleAunt = getMember(membersDict, path.NodeIds[0]); // B
                var greatGrandparent = getMember(membersDict, path.NodeIds[1]); // Z's parent
                var grandparent = getMember(membersDict, path.NodeIds[2]); // Y's parent
                var parent = getMember(membersDict, path.NodeIds[3]); // X's parent
                var greatNephewNiece = getMember(membersDict, path.NodeIds[4]); // A

                return greatUncleAunt != null && greatGrandparent != null && grandparent != null && parent != null && greatNephewNiece != null &&
                       greatUncleAunt.Gender == Gender.Male.ToString() && // B is male
                       greatGrandparent.Gender == Gender.Female.ToString(); // Through maternal female line
            },
            "chắt" // Great-nephew/niece of maternal great-uncle
        ));
        rules.Add(new RelationshipRule(
            new RelationshipPattern(new List<RelationshipType> { RelationshipType.Mother, RelationshipType.Mother, RelationshipType.Mother, RelationshipType.Child }),
            (path, membersDict) => {
                if (path.NodeIds.Count != 5) return false;
                var greatUncleAunt = getMember(membersDict, path.NodeIds[0]); // B
                var greatGrandparent = getMember(membersDict, path.NodeIds[1]); // Z's parent
                var grandparent = getMember(membersDict, path.NodeIds[2]); // Y's parent
                var parent = getMember(membersDict, path.NodeIds[3]); // X's parent
                var greatNephewNiece = getMember(membersDict, path.NodeIds[4]); // A

                return greatUncleAunt != null && greatGrandparent != null && grandparent != null && parent != null && greatNephewNiece != null &&
                       greatUncleAunt.Gender == Gender.Female.ToString() && // B is female
                       greatGrandparent.Gender == Gender.Female.ToString(); // Through maternal female line
            },
            "chắt" // Great-nephew/niece of maternal great-aunt
        ));

        return rules;
    }
}
