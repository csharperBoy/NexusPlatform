//src/modules/Identity/widgets/UserStatsWidget.tsx

import React from 'react';
import Card from '@/core/components/Card';

const UserStatsWidget: React.FC = () => {
  // می‌توانید داده‌ها را از API دریافت کنید (با useQuery)
  return (
    <Card className="p-4">
      <h3 className="font-bold text-lg">آمار کاربران</h3>
      <p className="text-3xl text-blue-600">۱۵۰</p>
      <p className="text-sm text-gray-500">کاربر فعال</p>
    </Card>
  );
};

export default UserStatsWidget;