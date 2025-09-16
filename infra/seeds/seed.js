// infra/seeds/seed.js
const { MongoClient, ObjectId } = require('mongodb');

const uri = "mongodb://localhost:27017";
const dbName = "family-tree";

async function seedData() {
  const client = new MongoClient(uri);

  try {
    await client.connect();
    const db = client.db(dbName);

    console.log(`Connected to MongoDB: ${dbName}`);

    // Clear existing data (optional, for fresh seeding)
    await db.collection('Families').deleteMany({});
    await db.collection('Members').deleteMany({});
    await db.collection('Relationships').deleteMany({});
    console.log('Cleared existing data.');

    // Create a sample family
    const family = {
      _id: new ObjectId(),
      name: "Gia Tộc Họ Nguyễn",
      address: "Hà Nội, Việt Nam",
      logoUrl: "https://example.com/nguyen-logo.png",
      description: "Gia tộc Nguyễn là một trong những gia tộc lâu đời nhất Việt Nam.",
      createdAt: new Date(),
      updatedAt: new Date()
    };
    await db.collection('Families').insertOne(family);
    console.log('Created sample family:', family.name);

    // Create sample members
    const members = [];
    const member1 = {
      _id: new ObjectId(),
      familyId: family._id,
      fullName: "Nguyễn Văn A",
      givenName: "A",
      dob: new Date("1950-01-15"),
      status: "deceased",
      avatarUrl: "https://example.com/nguyen-a.png",
      contact: { email: "a.nguyen@example.com", phone: "0901234567" },
      generation: 1,
      orderInFamily: 1,
      description: "Ông tổ đời thứ nhất.",
      createdAt: new Date(),
      updatedAt: new Date()
    };
    members.push(member1);

    const member2 = {
      _id: new ObjectId(),
      familyId: family._id,
      fullName: "Nguyễn Thị B",
      givenName: "B",
      dob: new Date("1955-03-20"),
      status: "deceased",
      avatarUrl: "https://example.com/nguyen-b.png",
      contact: { email: "b.nguyen@example.com", phone: "0901234568" },
      generation: 1,
      orderInFamily: 2,
      description: "Bà tổ đời thứ nhất.",
      createdAt: new Date(),
      updatedAt: new Date()
    };
    members.push(member2);

    const member3 = {
      _id: new ObjectId(),
      familyId: family._id,
      fullName: "Nguyễn Văn C",
      givenName: "C",
      dob: new Date("1975-07-01"),
      status: "alive",
      avatarUrl: "https://example.com/nguyen-c.png",
      contact: { email: "c.nguyen@example.com", phone: "0901234569" },
      generation: 2,
      orderInFamily: 1,
      description: "Con trai ông A và bà B.",
      createdAt: new Date(),
      updatedAt: new Date()
    };
    members.push(member3);

    const member4 = {
      _id: new ObjectId(),
      familyId: family._id,
      fullName: "Nguyễn Thị D",
      givenName: "D",
      dob: new Date("1980-11-10"),
      status: "alive",
      avatarUrl: "https://example.com/nguyen-d.png",
      contact: { email: "d.nguyen@example.com", phone: "0901234570" },
      generation: 2,
      orderInFamily: 2,
      description: "Con gái ông A và bà B.",
      createdAt: new Date(),
      updatedAt: new Date()
    };
    members.push(member4);

    const member5 = {
      _id: new ObjectId(),
      familyId: family._id,
      fullName: "Trần Văn E",
      givenName: "E",
      dob: new Date("1978-04-25"),
      status: "alive",
      avatarUrl: "https://example.com/tran-e.png",
      contact: { email: "e.tran@example.com", phone: "0901234571" },
      generation: 2,
      orderInFamily: 1,
      description: "Chồng bà D.",
      createdAt: new Date(),
      updatedAt: new Date()
    };
    members.push(member5);

    const member6 = {
      _id: new ObjectId(),
      familyId: family._id,
      fullName: "Nguyễn Văn F",
      givenName: "F",
      dob: new Date("2000-02-29"),
      status: "alive",
      avatarUrl: "https://example.com/nguyen-f.png",
      contact: { email: "f.nguyen@example.com", phone: "0901234572" },
      generation: 3,
      orderInFamily: 1,
      description: "Con trai ông C.",
      createdAt: new Date(),
      updatedAt: new Date()
    };
    members.push(member6);

    const member7 = {
      _id: new ObjectId(),
      familyId: family._id,
      fullName: "Nguyễn Thị G",
      givenName: "G",
      dob: new Date("2005-08-18"),
      status: "alive",
      avatarUrl: "https://example.com/nguyen-g.png",
      contact: { email: "g.nguyen@example.com", phone: "0901234573" },
      generation: 3,
      orderInFamily: 2,
      description: "Con gái ông C.",
      createdAt: new Date(),
      updatedAt: new Date()
    };
    members.push(member7);

    const member8 = {
      _id: new ObjectId(),
      familyId: family._id,
      fullName: "Nguyễn Văn H",
      givenName: "H",
      dob: new Date("2008-12-05"),
      status: "alive",
      avatarUrl: "https://example.com/nguyen-h.png",
      contact: { email: "h.nguyen@example.com", phone: "0901234574" },
      generation: 3,
      orderInFamily: 1,
      description: "Con trai bà D và ông E.",
      createdAt: new Date(),
      updatedAt: new Date()
    };
    members.push(member8);

    const member9 = {
      _id: new ObjectId(),
      familyId: family._id,
      fullName: "Nguyễn Thị I",
      givenName: "I",
      dob: new Date("2010-06-30"),
      status: "alive",
      avatarUrl: "https://example.com/nguyen-i.png",
      contact: { email: "i.nguyen@example.com", phone: "0901234575" },
      generation: 3,
      orderInFamily: 2,
      description: "Con gái bà D và ông E.",
      createdAt: new Date(),
      updatedAt: new Date()
    };
    members.push(member9);

    const member10 = {
      _id: new ObjectId(),
      familyId: family._id,
      fullName: "Nguyễn Văn K",
      givenName: "K",
      dob: new Date("2020-09-01"),
      status: "alive",
      avatarUrl: "https://example.com/nguyen-k.png",
      contact: { email: "k.nguyen@example.com", phone: "0901234576" },
      generation: 4,
      orderInFamily: 1,
      description: "Con trai ông F.",
      createdAt: new Date(),
      updatedAt: new Date()
    };
    members.push(member10);

    await db.collection('Members').insertMany(members);
    console.log(`Created ${members.length} sample members.`);

    // Create sample relationships
    const relationships = [
      // Parents of C and D
      { _id: new ObjectId(), familyId: family._id, memberId: member1._id, relationType: "parent", targetMemberId: member3._id, createdAt: new Date(), updatedAt: new Date() },
      { _id: new ObjectId(), familyId: family._id, memberId: member2._id, relationType: "parent", targetMemberId: member3._id, createdAt: new Date(), updatedAt: new Date() },
      { _id: new ObjectId(), familyId: family._id, memberId: member1._id, relationType: "parent", targetMemberId: member4._id, createdAt: new Date(), updatedAt: new Date() },
      { _id: new ObjectId(), familyId: family._id, memberId: member2._id, relationType: "parent", targetMemberId: member4._id, createdAt: new Date(), updatedAt: new Date() },
      // Spouse of D
      { _id: new ObjectId(), familyId: family._id, memberId: member4._id, relationType: "spouse", targetMemberId: member5._id, startDate: new Date("1999-05-01"), createdAt: new Date(), updatedAt: new Date() },
      { _id: new ObjectId(), familyId: family._id, memberId: member5._id, relationType: "spouse", targetMemberId: member4._id, startDate: new Date("1999-05-01"), createdAt: new Date(), updatedAt: new Date() },
      // Children of C
      { _id: new ObjectId(), familyId: family._id, memberId: member3._id, relationType: "parent", targetMemberId: member6._id, createdAt: new Date(), updatedAt: new Date() },
      { _id: new ObjectId(), familyId: family._id, memberId: member3._id, relationType: "parent", targetMemberId: member7._id, createdAt: new Date(), updatedAt: new Date() },
      // Children of D and E
      { _id: new ObjectId(), familyId: family._id, memberId: member4._id, relationType: "parent", targetMemberId: member8._id, createdAt: new Date(), updatedAt: new Date() },
      { _id: new ObjectId(), familyId: family._id, memberId: member5._id, relationType: "parent", targetMemberId: member8._id, createdAt: new Date(), updatedAt: new Date() },
      { _id: new ObjectId(), familyId: family._id, memberId: member4._id, relationType: "parent", targetMemberId: member9._id, createdAt: new Date(), updatedAt: new Date() },
      { _id: new ObjectId(), familyId: family._id, memberId: member5._id, relationType: "parent", targetMemberId: member9._id, createdAt: new Date(), updatedAt: new Date() },
      // Child of F
      { _id: new ObjectId(), familyId: family._id, memberId: member6._id, relationType: "parent", targetMemberId: member10._id, createdAt: new Date(), updatedAt: new Date() },
    ];
    await db.collection('Relationships').insertMany(relationships);
    console.log(`Created ${relationships.length} sample relationships.`);

    // Create indexes
    await db.collection('Families').createIndex({ name: "text", address: "text" });
    await db.collection('Members').createIndex({ fullName: "text", familyId: 1, generation: 1, "contact.email": 1 });
    await db.collection('Relationships').createIndex({ familyId: 1, memberId: 1, targetMemberId: 1, relationType: 1 });
    console.log('Created indexes.');

  } finally {
    await client.close();
    console.log('MongoDB connection closed.');
  }
}

seedData().catch(console.error);
