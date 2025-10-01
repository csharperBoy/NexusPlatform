import {
  Box,
  Typography,
  TextField,
  MenuItem,
  CircularProgress,
  FormGroup,
  FormControlLabel,
  Checkbox,
  Button,
} from '@mui/material';
import { useEffect, useImperativeHandle, useState, forwardRef } from 'react';
import { GetIndicatorList, GetIndicatorClaimsForIndicator, SaveIndicatorClaims } from '../api/IndicatorCollection';
import { getIndicatorClaims } from '../api/LookUpCollection';
import { getUserList } from '../api/UserCollection';
import { SaveIndicatorClaimsRequest } from '../models/Indicator/SaveIndicatorClaims';
import { GetIndicatorListResponse } from '../models/Indicator';
import { GetLookupListResponse } from '../models/Lookup/GetLookupList';
import { UserModel } from '../models/User';

export interface IndicatorClaimAssignmentRef {
  getRequest: () => SaveIndicatorClaimsRequest | null;
}

interface Props {
  initialIndicatorId?: string;
  onSave?: (req: SaveIndicatorClaimsRequest) => Promise<void>;
  onNext?: () => void;
  onBack?: () => void;
}

const IndicatorAssignToClaim = forwardRef<IndicatorClaimAssignmentRef, Props>(
  ({ initialIndicatorId, onSave, onNext, onBack }, ref) => {
    const [indicatorId, setIndicatorId] = useState(initialIndicatorId ?? '');
    const [selectedUserId, setSelectedUserId] = useState('');
    const [claimsIds, setClaimsIds] = useState<number[]>([]);
    const [indicators, setIndicators] = useState<GetIndicatorListResponse[]>([]);
    const [claims, setClaims] = useState<GetLookupListResponse[]>([]);
    const [users, setUsers] = useState<UserModel[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
      (async () => {
        const [indicatorList, claimList, userList] = await Promise.all([
          GetIndicatorList(),
          getIndicatorClaims(),
          getUserList(),
        ]);

        setIndicators(indicatorList);
        setClaims(claimList);
        setUsers(userList);

        if (initialIndicatorId) {
          const assigned = await GetIndicatorClaimsForIndicator(initialIndicatorId);
          setClaimsIds(assigned.map(c => c.id));
          setIndicatorId(initialIndicatorId);
        }

        setLoading(false);
      })();
    }, [initialIndicatorId]);

    useImperativeHandle(ref, () => ({
      getRequest: (): SaveIndicatorClaimsRequest | null => {
        if (!indicatorId || !selectedUserId) return null;
        return {
          indicatorId,
          userId: selectedUserId,
          claimsIds,
        };
      },
    }));

    const [isSaving, setIsSaving] = useState(false);

    const handleSave = async () => {
      const req: SaveIndicatorClaimsRequest | null = (indicatorId && selectedUserId) ? { indicatorId, userId: selectedUserId, claimsIds } : null;
      if (!req) return;
      try {
        setIsSaving(true);
        if (onSave) await onSave(req);
        else await SaveIndicatorClaims(req);
      } finally {
        setIsSaving(false);
      }
    };

    const handleSaveAndNext = async () => {
      await handleSave();
      if (onNext) onNext();
    };

    const handleIndicatorChange = async (value: string) => {
      setIndicatorId(value);
      const assigned = await GetIndicatorClaimsForIndicator(value);
      setClaimsIds(assigned.map(c => c.id));
    };

    const toggleClaim = (claimId: number) => {
      setClaimsIds(prev =>
        prev.includes(claimId)
          ? prev.filter(id => id !== claimId)
          : [...prev, claimId]
      );
    };

    if (loading) return <CircularProgress />;

    return (
      <Box>
        <Typography variant="h6" mb={2}>
          تخصیص شاخص به ادعاها و کاربران
        </Typography>

        <TextField
          select
          fullWidth
          label="انتخاب شاخص"
          value={indicatorId}
          onChange={e => handleIndicatorChange(e.target.value)}
          disabled={Boolean(initialIndicatorId)}
          sx={{ mb: 2 }}
        >
          {indicators.map(ind => (
            <MenuItem key={ind.id} value={ind.id}>
              {ind.title}
            </MenuItem>
          ))}
        </TextField>

        <TextField
          select
          fullWidth
          label="انتخاب کاربر"
          value={selectedUserId}
          onChange={e => setSelectedUserId(e.target.value)}
          sx={{ mb: 3 }}
        >
          {users.map(user => (
            <MenuItem key={user.id} value={user.id}>
              {user.displayName}
            </MenuItem>
          ))}
        </TextField>

        <Typography variant="subtitle1" mb={1}>
          ادعاها:
        </Typography>

        <FormGroup>
          {claims.map(claim => (
            <FormControlLabel
              key={claim.id}
              control={
                <Checkbox
                  checked={claimsIds.includes(claim.id)}
                  onChange={() => toggleClaim(claim.id)}
                />
              }
              label={claim.display}
            />
          ))}
        </FormGroup>

        <Box sx={{ mt: 3, display: 'flex', justifyContent: 'space-between' }}>
          <Box>
            {onBack && (
              <Button variant="outlined" onClick={onBack} disabled={isSaving}>
                بازگشت
              </Button>
            )}
          </Box>

          <Box>
            <Button variant="contained" onClick={handleSave} disabled={isSaving} sx={{ mr: 1 }}>
              ثبت
            </Button>
            {onNext && (
              <Button variant="contained" color="primary" onClick={handleSaveAndNext} disabled={isSaving}>
                ثبت و ادامه
              </Button>
            )}
          </Box>
        </Box>
      </Box>
    );
  }
);

export default IndicatorAssignToClaim;
