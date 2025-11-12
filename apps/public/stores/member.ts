import { defineStore } from 'pinia';

interface Member {
  id: number;
  name: string;
  birthDate: string;
  deathDate?: string;
  imageUrl: string;
  shortBio: string;
}

interface MemberState {
  featuredMembers: Member[];
  searchResults: Member[];
  loading: boolean;
  error: string | null;
}

export const useMemberStore = defineStore('member', {
  state: (): MemberState => ({
    featuredMembers: [],
    searchResults: [],
    loading: false,
    error: null,
  }),
  actions: {
    async fetchFeaturedMembers() {
      this.loading = true;
      this.error = null;
      try {
        // Simulate API call
        await new Promise(resolve => setTimeout(resolve, 1000));
        this.featuredMembers = [
          {
            id: 1,
            name: 'Nguyễn Văn A',
            birthDate: '1950-01-01',
            deathDate: '2020-12-31',
            imageUrl: 'https://via.placeholder.com/150',
            shortBio: 'Ông Nguyễn Văn A là một người con hiếu thảo, một người cha mẫu mực và một người chồng tận tụy. Ông đã cống hiến cả đời mình cho sự phát triển của gia đình và quê hương. Ông luôn được mọi người yêu mến và kính trọng.',
          },
          {
            id: 2,
            name: 'Trần Thị B',
            birthDate: '1955-03-15',
            imageUrl: 'https://via.placeholder.com/150',
            shortBio: 'Bà Trần Thị B là một người phụ nữ đảm đang, tháo vát. Bà đã cùng chồng xây dựng một gia đình hạnh phúc và nuôi dạy các con nên người. Bà luôn là chỗ dựa vững chắc cho cả gia đình.',
          },
          {
            id: 3,
            name: 'Lê Văn C',
            birthDate: '1980-07-20',
            imageUrl: 'https://via.placeholder.com/150',
            shortBio: 'Anh Lê Văn C là một người trẻ năng động, sáng tạo. Anh luôn nỗ lực học hỏi và phát triển bản thân. Anh là niềm tự hào của gia đình và là tấm gương cho các thế hệ sau.',
          },
        ];
      } catch (err: any) {
        this.error = err.message || 'Failed to fetch featured members.';
      } finally {
        this.loading = false;
      }
    },
    async searchMembers(query: string) {
      this.loading = true;
      this.error = null;
      try {
        // Simulate API call
        await new Promise(resolve => setTimeout(resolve, 1000));
        const allMembers = [
          ...this.featuredMembers,
          { id: 4, name: 'Phạm Thị D', birthDate: '1982-11-05', imageUrl: 'https://via.placeholder.com/150', shortBio: 'Phạm Thị D là một thành viên tích cực trong cộng đồng, luôn sẵn lòng giúp đỡ mọi người.' },
          { id: 5, name: 'Hoàng Văn E', birthDate: '1970-02-28', imageUrl: 'https://via.placeholder.com/150', shortBio: 'Hoàng Văn E là một nghệ nhân tài hoa, có nhiều đóng góp cho văn hóa truyền thống.' },
        ];
        this.searchResults = allMembers.filter(member =>
          member.name.toLowerCase().includes(query.toLowerCase())
        );
      } catch (err: any) {
        this.error = err.message || 'Failed to search members.';
      } finally {
        this.loading = false;
      }
    },
  },
});
