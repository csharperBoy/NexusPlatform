import React, { useState } from 'react';
import { ChangePassword as ChangePasswordApi } from '../api/UserCollection';
import { ChangePasswordRequest } from '../models/User/ChangePassword'; // Adjust the import path as necessary
const ChangePassword: React.FC = () => {
    const [currentPassword, setCurrentPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        setSuccess('');

        if (!currentPassword || !newPassword || !confirmPassword) {
            setError('لطفا تمام فیلدها را پر کنید.');
            return;
        }
        if (newPassword !== confirmPassword) {
            setError('رمز های عبور جدید مطابقت ندارند.');
            return;
        }

        const request: ChangePasswordRequest = {
            oldPassword: currentPassword,
            newPassword: newPassword
        };
        try {
            const result = await ChangePasswordApi(request);
            if (result) {
                setSuccess('رمز عبور با موفقیت تغییر یافت.');
                setCurrentPassword('');
                setNewPassword('');
                setConfirmPassword('');
            } else {
                setError('تغییر رمز عبور ناموفق بود.');
            }
        } catch (err) {
            setError('خطایی در حین تغییر رمز عبور رخ داد.');
        }
    };

    return (
        <div className="max-w-md mx-auto mt-16 p-8 bg-white rounded-xl shadow-lg border border-gray-200">
            <h2 className="text-2xl font-bold mb-6 text-center text-primary">تغییر رمز عبور</h2>
            <form onSubmit={handleSubmit} className="space-y-5">
                <div>
                    <label className="block mb-2 text-right font-medium text-gray-700">رمز عبور فعلی</label>
                    <input
                        type="password"
                        value={currentPassword}
                        onChange={e => setCurrentPassword(e.target.value)}
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary text-right"
                        required
                        placeholder="رمز عبور فعلی را وارد کنید"
                    />
                </div>
                <div>
                    <label className="block mb-2 text-right font-medium text-gray-700">رمز عبور جدید</label>
                    <input
                        type="password"
                        value={newPassword}
                        onChange={e => setNewPassword(e.target.value)}
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary text-right"
                        required
                        placeholder="رمز عبور جدید را وارد کنید"
                    />
                </div>
                <div>
                    <label className="block mb-2 text-right font-medium text-gray-700">تکرار رمز عبور جدید</label>
                    <input
                        type="password"
                        value={confirmPassword}
                        onChange={e => setConfirmPassword(e.target.value)}
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary text-right"
                        required
                        placeholder="رمز عبور جدید را دوباره وارد کنید"
                    />
                </div>
                {error && <div className="text-red-600 text-right mb-2">{error}</div>}
                {success && <div className="text-green-600 text-right mb-2">{success}</div>}
                <button type="submit" className="w-full py-2 px-4 bg-primary text-white rounded-lg font-semibold hover:bg-primary-dark transition">تغییر رمز عبور</button>
            </form>
        </div>
    );
};

export default ChangePassword;